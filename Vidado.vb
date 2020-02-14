'#Language "WWB-COM"
Option Explicit
'See https://github.com/KofaxRPA/Vidado for source code
'In the Menu "Edit/References..." Add a reference to "Microsoft XML, 6.0"
'In the Menu "Edit/References..." Add a reference to "Kofax Cascade Advanced Zone Locator"
'This script calls Vidado Read engine, https://api.vidado.ai/portal/ for images less than 100,000 pixels.
'Configure your Advanced Zone Locators as normal with registration, zones and anchors.
'Give each field you want to send to Vidado an OMR profile with the name "Vidado". The script will replace the OMR result with Vidado's OCR
'You will need a VidadoAPIKey and add it to the Project's Script Variables "VidadoAPIKey"

Private Declare Function GetTickCount Lib "kernel32" () As Long ' milliseconds

Public Sub Document_AfterLocate(ByVal pXDoc As CASCADELib.CscXDocument, ByVal LocatorName As String)
   If pXDoc.ExtractionClass="" Then Err.Raise(456,,"The XDocument needs to be classified before trying to extract.")
   If Project.ClassByName(pXDoc.ExtractionClass).Locators.ItemByName(LocatorName).LocatorMethod.Name="AdvZone" Then ' if this is an Advanced Zone Locator
      AZL_Vidado(pXDoc,LocatorName,Project.ScriptVariables.ItemByName("VidadoAPIKey").Value)
   End If
End Sub

Sub AZL_Vidado(ByVal pXDoc As CASCADELib.CscXDocument,ByVal LocatorName As String,VidadoAPIKey As String)
   'This runs at the End of the Advanced Zone Locator. If any zone uses a profile with the name "Vidado" the image will be sent to Vidado
   'This uses the Zones after AZL has registered them.
   Dim AZL As CscAdvZoneLocator, Zones As CscAdvZoneLocZones, Zone As CscAdvZoneLocZone, Z As Long, Alts As CscXDocFieldAlternatives, S As Long
   Dim Confidence As Double, Image As CscImage, Page As CscImage, SubField As CscAdvZoneLocSubfield, ImageFileName As String
   Set AZL=Project.ClassByName(pXDoc.ExtractionClass).Locators.ItemByName(LocatorName).LocatorMethod
   Set Alts=pXDoc.Locators.ItemByName(LocatorName).Alternatives
   For Z=0 To AZL.Zones.Count-1
      Set Zone=AZL.Zones(Z) 'We have to trace through the Zone Locator from the Zone to the Zone-Subfield Mapping to the AZL'Subfield to the Locator's Alternative(0)'s Subfield
      If Project.RecogProfiles.ItemByID(Zone.RecogProfileId).Name="Vidado" Then
         If AZL.Mappings.ItemExistsByZoneId(Zone.ID) Then
            Set SubField=AZL.SubFields.ItemByID(AZL.Mappings.ItemByZoneId(Zone.ID).SubfieldId)
            For S=0 To AZL.SubFields.Count-1
               If SubField.Name=Alts(0).SubFields(S).Name Then
                  With Alts(0).SubFields(S)
                     Set Page=pXDoc.CDoc.Pages(.PageIndex).GetImage
                     Set Image=New CscImage
                     Image.CreateImage(Image_GetColorFormat(Page),.Width,.Height,Page.XResolution,Page.YResolution)
                     Image.CopyRect(Page,.Left,.Top,0,0,.Width,.Height)
                     ImageFileName= Environ("temp") & "\" & GUID_Create() & ".png" 'we need a unique file name for parallelization in KT server
                     Image.Save(ImageFileName,CscImgFileFormatPNG)
                     If .Width*.Height>100000 Then Image_Shrink(ImageFileName,.Width,.Height)
                     .Text=Vidado_API(ImageFileName,VidadoAPIKey,Confidence)
                     .Confidence=Confidence
                     Kill ImageFileName ' delete the temp image after Vidado has finished
                     Exit For
                  End With
               End If
            Next
         End If
      End If
   Next
End Sub

Private Function Image_GetColorFormat(Image As CscImage) As CscImageColorFormat
      Select Case Image.BitsPerSample
         Case 1
            Return CscImgColFormatBinary
         Case 4
            Return CscImgColFormatGray4
         Case 8
            Return CscImgColFormatGray8
         Case 16
            Return CscImgColFormatGray16
         Case 24
            Return CscImgColFormatRGB24
      End Select
End Function

Private Sub Image_Shrink(ImageFileName As String, Width As Long, Height As Long)
   Dim WIA As Object 'WIA.ImageFile
   Dim IP As Object 'ImageProcess
   Dim Scale As Double 'This will shrink an image file to just less then 100,000 pixels.
   Scale=100000/Width/Height*0.98
   Set WIA = CreateObject("WIA.ImageFile")
   Set IP = CreateObject("WIA.ImageProcess")
   IP.Filters.Add IP.FilterInfos("Scale").FilterID
   IP.Filters(1).Properties("MaximumWidth") = Width*Scale
   IP.Filters(1).Properties("MaximumHeight") = Height*Scale
   WIA.LoadFile(ImageFileName)
   Set WIA = IP.Apply(WIA)
   Kill(ImageFileName)
   WIA.SaveFile(ImageFileName)
End Sub

Dim Timestamp As Long
Private Function Vidado_API(ImageFileName As String, VidadoAPIKey As String, ByRef Confidence As Double) As String
   Dim Filename As String, JSON() As String
   Dim Boundary As String, Body As String, Bytes() As Byte, Now As Long
   Open ImageFileName For Binary Access Read As #1
   ReDim Bytes(0 To LOF(1) - 1)
   Get #1 ,, Bytes ' read the PNG file into a byte array
   Close #1
   TimeStamp=GetTickCount()
   Boundary = "------------------------b944533fb31e85b5"   'HTTP multipart boundary https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Type
   Body = Body & "--" & Boundary & vbCrLf
   Body = Body & "Content-Disposition: form-data; name=""image""; filename=""001.png""" & vbCrLf
   Body = Body & "Content-Type: image/png" & vbCrLf & vbCrLf
   Body = Body & StrConv(Bytes,vbFromANSIBytes)
   Body = Body & vbCrLf & "--" & Boundary & "--" & vbCrLf
   Do
      With New MSXML2.XMLHTTP60
         .Open("POST","https://api.vidado.ai/read/text",varAsync:=False)
         .setRequestHeader("accept", "application/json")
         .setRequestHeader("Authorization", VidadoAPIKey)
         .setRequestHeader("Content-Length", UBound(Bytes))   'LenB is wrong, it must be Len
         .setRequestHeader("Content-Type", "multipart/form-data; boundary=" & Boundary)
         Bytes=StrConv(Body,vbANSIBytes)
         Now=GetTickCount()
         Do While Now-Timestamp <100 ' the trial license only allows 1 call per second, so we wait 1500 ms
            DoEvents
            Now=GetTickCount()
         Loop
         .send(Bytes)  'Send the image to Vidado
         Confidence=0.0
         If .status=200 Then  ' Vidado returned success
            JSON=Split(.responseText,"""") ' very cheap JSON parser! split the JSON results from Vidado into an array, using " as delimiter
            If UBound(JSON)=14 Then ' a successful Vidado result without errors has 14 elements
               Confidence=CDbl(Mid(JSON(6),2,Len(JSON(6))-2))  ' super cheap JSON parser to get the OCR confidence
               Return JSON(3) ' the OCR text
            End If
         ElseIf .status=405 Then
            Return "Error 404 page not found"
         ElseIf .status <> 429 Then 'Vidado returned an error
            Return ("Error " + CStr(.status) & " : " & Split(.responseText,"""")(3) )
         End If
         'we get here only on a 429 error "Rate limit exceeded" when we were too quick!
         Confidence=0
      End With
   Loop
End Function

Function GUID_Create() As String
   Dim TypeLib As Object
   Set TypeLib = CreateObject("Scriptlet.TypeLib")
   Return Mid(TypeLib.GUID,2,36) ' format is {24DD18D4-C902-497F-A64B-28B2FA741661}
End Function

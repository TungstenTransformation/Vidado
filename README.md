# Integration of Vidado Read API into Kofax Transformation
<img src="https://vidado.ai/wp-content/themes/vidado/images/Vidado-logo-blue.png" alt="Vidado Logo" height="45">
<img src="https://www.kofax.com/-/media/Images/Global/Header/logo_header.svg" alt="Kofax Logo" height="45">
Vidado Read API, https://api.vidado.ai/portal/, is an extremely powerful OCR engine. Vidado confidently claims that it reads better than a human. You can test it right now online at https://vidado.ai/try-it-out/!  
This guide will show you how to integrate Vidado Read API into the Advanzed Zone Locator in Kofax Transformation.  

## Compatible with all versions of
* Kofax Total Agility
* Kofax Transformation Modules in Kofax Capture
* Kofax RPA (since 10.3)
* Kofax Mobile Capture
* Kofax RTTS
* Kofax Transformation Toolkit

## Vidado Read API Features
* Cloud-based OCR engine. Vidado does not keep any images or results after processing.
* Vidado READ API supports **zones** and not **pages** - each image must be smaller than 100,000 pixels, which is 1.1 inch² or 7.1 cm² at 300 dpi. *The script will dynamically shrink images that are larger than 100,000 pixels.*
* The engine is a Deep Neural Network trained on 1,000,000,000 hand-checked field samples in *English*.
  * It was trained to read words on forms and hence it ignores labels. *in the example below "christina" is found and "witness signature" is ignored*  
![image](https://user-images.githubusercontent.com/47416964/74426760-33b6e680-4e56-11ea-81ea-b5dab692f1ab.png)
* Vidado claims over 90% field level accuracy or better than a human.
  * Vidado only returns results in lower case. It does not return accents.
  * You can expect excellent results on Latin languages without accents - eg Spanish, Dutch, Italian, Swahili, Indonesian.
* It works on black&white, greyscale and color images.
* Vidado supports jpeg, jpg, gif, png. *The script uses png because it supports B&W, grey and color losslessly*
* Only horizontal text, nor can vertical text be read.
* There is no deskew, but the engine was trained on samples with ±5° skew.
* no image cleanup at all is required - smudges, lines, boxes, shadings are all ignored.
* You cannot train Vidado, but as a Clound System it is constantly being improved.
* The trial license is limited to 1 call per second.

## Configuration steps
1. Register for an API key at https://api.vidado.ai/portal/. This will give you a trial key for two weeks allowing for 1000 images/day and 1 image/second.
1. Create or Open a Kofax Transformation Project in Project Builder or Transformation Designer.
1. Add the [script](Vidado.vb) to the class where your Advanced Zone Locator is.  
*Note you can have multiple Advanced Zone Locators in your class. The script will run on them all.*
1. In the Script Editor on the Menu Edit/References.. add a Reference to **Kofax Cascade Advanced Zone Locator 4.0**
1. In the Script Editor on the Menu Edit/References.. add a Reference to **Microsoft XML, 6.0**
1. Add your Vidado API Key to the Project's Script Variables, with a script variable named **VidadoAPIKey**
![image](https://user-images.githubusercontent.com/47416964/74356916-695cc080-4dbf-11ea-8aa6-f6107b48e121.png)
1. Create a new Zone OMR profile called **Vidado** in **Project Settings/Recognotion/Zone Profile...**
![image](https://user-images.githubusercontent.com/47416964/74357087-a88b1180-4dbf-11ea-96a9-60c026313646.png)  
*Tip! Set your Vidado Profile to be the default profile, and it will be autoselected for new OMR Zones.*
![image](https://user-images.githubusercontent.com/47416964/74358438-b6da2d00-4dc1-11ea-9890-81b4dd8f3576.png)
1. Inside your Advanced Zone Locator, select the Vidado Profile for each zone you want to send to Vidado
1. **IMPORTANT** Make sure you classify your documents BEFORE testing the Advanced Zone Locator.
1. Press "Test" on your Advanced Zone Locator to see Vidado's magic. **You cannot test Vidado inside the Zone settings, but you can inside the locator** *You do not need any image cleanup profiles*
![image](https://user-images.githubusercontent.com/47416964/74366173-c8c2cc80-4dcf-11ea-8f76-fd73810e1b00.png)

## Vidado License Tiers
Tier|Cost/month|Quota/day|Calls/second
----|----------|---------|-----------
Developer|Free|1,000|1
Basic|$499|35,000|3
Scale|$999|75,000|6
Enterprise|varies|custom|20




# Integration of Vidado Read into Kofax Transformation
<img src="https://vidado.ai/wp-content/themes/vidado/images/Vidado-logo-blue.png" alt="Vidado Logo" height="45">
<img src="https://www.kofax.com/-/media/Images/Global/Header/logo_header.svg" alt="Kofax Logo" height="45">
Vidado Read, (https://api.vidado.ai/portal/), is an extremely powerful OCR engine. Vidado confidently claims that it **always** reads better than a human  
This guide will show you how to integrate Vidado Read API into the Advanzed Zone Locator in Kofax Transformation  

## compatible with all versions of
* Kofax Total Agility
* Kofax Transformation Modules
* Kofax RPA
* Kofax Mobile Capture
* Kofax RTTS

## Vidado Features
* Cloud-based OCR engine. Vidado does not keep any images or results after processing.
* Deep Neural Network trained on 1,000,000,000 field samples in *English*.
* Vidado claims over 90% field level accuracy or better than a human.
  * You can expect excellent results on Latin languages without accents - eg Spanish, Dutch, Italian, Swahili, Indonesian.
* works on black&white, greyscale and color images.
* images must be smaller than 100,000 pixels, which is 1.1 inch² or 7.1 cm² at 300 dpi.
* The trial license is limited to 1 call per second.

## Configuration steps
1. Register for an API key at [https://api.vidado.ai/portal/]. This will give you a trial key allowing for 1000 images/day
1. Create or Open a Kofax Transformation Project in Project Builder or Transformation Designer.
1. Add the [script](https://github.com/KofaxRPA/Vidado/blob/master/Vidado.vb) to the class where your Advanced Zone Locator is.  
*Note that the script is looking for locators called "AZL"*
1. Add your Vidado API Key to the Project's Script Variables, with a script variable named **VidadoAPIKey**
![image](https://user-images.githubusercontent.com/47416964/74356916-695cc080-4dbf-11ea-8aa6-f6107b48e121.png)
1. Create a new Zone OMR profile called **Vidado** in **Project Settings/Recognotion/Zone Profile...**
![image](https://user-images.githubusercontent.com/47416964/74357087-a88b1180-4dbf-11ea-96a9-60c026313646.png)  
Tip! Set your Vidado Profile to be the default profile, and it will be autoselected for new OMR Zones.
![image](https://user-images.githubusercontent.com/47416964/74358438-b6da2d00-4dc1-11ea-9890-81b4dd8f3576.png)
1. inside your Advanced Zone Locator, slect the Vidado Profile fo each zone you want to send to Vidado
1. Press "Test" on your Advanced Zone Locator to see Vidado's magic. **You cannot test Vidado inside the Zone settings, but you can inside the locator**


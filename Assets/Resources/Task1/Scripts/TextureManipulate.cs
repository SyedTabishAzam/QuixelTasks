using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;

public class TextureManipulate : MonoBehaviour {

    public GameObject PlaneA;
    public GameObject PlaneB;
    public GameObject PlaneC;
    public TextAsset ConfigFile;

    private string textureAPath, textureBPath;
    private Material planeAmaterial, planeBmaterial;

    // Use this for initialization
    void Start ()
    {
        planeAmaterial = PlaneA.transform.GetChild(0).GetComponent<Renderer>().material;
        planeBmaterial = PlaneB.transform.GetChild(0).GetComponent<Renderer>().material;

        //On start of game, Read config file and create render textures from loaded textures
        if (ExtractData())
        {
            CreateRenderTextures();
        }
    }

    bool ExtractData()
    {
        //Read XML file and parse it for file location of textures (.jpg)
        XmlDocument configFile = new XmlDocument();
  
        if(ConfigFile)
        {
            configFile.Load(Application.dataPath + "/Resources/Task1/" + ConfigFile.name + ".xml");
            XmlNode xmlRootNode = configFile.DocumentElement;
            XmlNodeList xmlNodeList = xmlRootNode.ChildNodes;

            textureAPath = xmlNodeList[0].InnerText;
            textureBPath = xmlNodeList[1].InnerText;

            return true;
        }
        Debug.LogError("Config file not selected. Texture wont be loaded");
        return false;

    }

    void CreateRenderTextures()
    {
        //Load image file as Texture

        //Create render texture for plane A's material
        var textureForA = Resources.Load<Texture2D>(textureAPath) as Texture;

        CustomRenderTexture rtForA = new CustomRenderTexture(256, 256);
        rtForA.initializationTexture = textureForA;
        rtForA.initializationMode = CustomRenderTextureUpdateMode.Realtime;
        rtForA.updateMode = CustomRenderTextureUpdateMode.Realtime;
        rtForA.material = planeAmaterial;
        rtForA.wrapMode = TextureWrapMode.Repeat;
        rtForA.Create();

        planeAmaterial.SetTexture("_MainTex", rtForA);

        //Create render texture for plane B's material
        var textureForB = Resources.Load<Texture2D>(textureBPath) as Texture;

        CustomRenderTexture rtForB = new CustomRenderTexture(256, 256);
        rtForB.initializationTexture = textureForB;
        rtForB.initializationMode = CustomRenderTextureUpdateMode.Realtime;
        rtForB.updateMode = CustomRenderTextureUpdateMode.Realtime;
        rtForB.material = planeBmaterial;
        rtForB.wrapMode = TextureWrapMode.Repeat;
        rtForB.Create();

        planeBmaterial.SetTexture("_MainTex", rtForB);
    }

    

    // Update is called once per frame
    void Update () {
		
      
	}

   public void SubtractTexture()
    {
        //On button click - get both the textures for plane A and plane B
        RenderTexture planeBTexture = planeBmaterial.GetTexture("_MainTex") as RenderTexture;
        RenderTexture planeATexture = planeAmaterial.GetTexture("_MainTex") as RenderTexture;
        

        //Apply plane A texture as Primary and plane B texture as Secondry texture in Plane C shadder
        PlaneC.transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex2",planeBTexture);
        PlaneC.transform.GetChild(0).GetComponent<Renderer>().material.SetTexture("_MainTex", planeATexture);

        //Once the texture is applied, save it in a .png file
        StartCoroutine(GetAndSaveTexture());

    }

    IEnumerator GetAndSaveTexture()
    {
        //The couritinemethod is put in corotuine to ensure that rendering of PlaneC material is finsihed before the texture is saved

        yield return new WaitForEndOfFrame();

        //Width and height of returning texture
        int width = 800;
        int height = 800;

        //Create output texture to save shaders output
        Texture2D outputTex = new Texture2D(width, height, TextureFormat.ARGB32, false);

        //Get plane texture
        Texture PlaneCTexture = PlaneC.transform.GetChild(0).GetComponent<Renderer>().material.mainTexture as Texture;

        //Create a new render texture to write an offscreen buffer
        RenderTexture buffer = new RenderTexture(
                               width,
                               height,
                               0,                            // No depth/stencil buffer
                               RenderTextureFormat.ARGB32,   // Standard colour format
                               RenderTextureReadWrite.Linear // No sRGB conversions
                           );

        //Bind texture to the creater buffer
        Graphics.Blit(PlaneCTexture, buffer);
        RenderTexture.active = buffer; 
        
        //Capture whole screen texture and start reading from top left pixel
        outputTex.ReadPixels(
                  new Rect(0, 0, width, height), 
                  0, 0,                         
                  false                          
        );

        //Create bytes array to save encoded png
        byte[] bytes;
        bytes = outputTex.EncodeToPNG();

        //Write to file
        string path = Application.dataPath + "/../PlaneCTexture.png";
        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path);
        Debug.Log("File saved at: " + path);
    }

   
}

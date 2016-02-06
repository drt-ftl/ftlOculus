using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class camScript : MonoBehaviour
{
    public GameObject button;
    public GameObject cursor;
    public Material Mat;
    public Dropdown materialDD;
    public Dropdown skyboxDD;
    public Dropdown landscapeDD;
    public Dropdown propDD;

    private GameObject _cursor;
    public static MakeMesh MM;

    public static GameObject mainMenu;
    public static GameObject folderWindow;
    public static GameObject sceneWindow;
    private List<List<GameObject>> fileButtons = new List<List<GameObject>>();
    private StlInterpreter stlInterpreter;
    public static List<Vector3> currentVertices = new List<Vector3>();
    public static Vector3 Min = new Vector3(1000, 1000, 1000);
    public static Vector3 Max = new Vector3(-1000, -1000, -1000);
    public List<Material> skyboxes = new List<Material>();
    public List<GameObject> landscapes = new List<GameObject>();
    public List<GameObject> props = new List<GameObject>();
    public static GameObject currentLandscape;
    public static GameObject currentProp;
    private float mouseYoffset = -7;

    void Start ()
    {
        stlInterpreter = new StlInterpreter();
        mainMenu = GameObject.Find("Main Menu");
        folderWindow = GameObject.Find("Folder Window");
        sceneWindow = GameObject.Find("Scene Window");

        folderWindow.GetComponent<PanelFades>().FadeOut();
        sceneWindow.GetComponent<PanelFades>().FadeOut();
        _cursor = Instantiate(cursor) as GameObject;
        _cursor.transform.SetParent(mainMenu.transform);
        var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y + mouseYoffset, -1f));
        var rot = new Quaternion(0, 0, 0, 0);
        _cursor.transform.localPosition = pos;
        _cursor.transform.localRotation = rot;
        MM = GameObject.Find("MESH").GetComponent<MakeMesh>();
        MM.material = Mat;
        MM.Begin();

        for (int i = 0; i < landscapes.Count; i++)
        {
            var l = Instantiate(landscapes[i]);
            l.name = landscapes[i].name;
            l.active = false;
            landscapes[i] = l;
        }

        for (int i = 0; i < props.Count; i++)
        {
            var p = Instantiate(props[i]);
            p.name = props[i].name;
            p.active = false;
            props[i] = p;
        }

        var options = new List<Dropdown.OptionData>(skyboxes.Count);
        foreach (var skybox in skyboxes)
        {
            var newOption = new Dropdown.OptionData(skybox.name);
            options.Add(newOption);
        }
        skyboxDD.options = options;

        options = new List<Dropdown.OptionData>(MM.materials.Count);
        foreach (var mat in MM.materials)
        {
            var newOption = new Dropdown.OptionData(mat.name);
            options.Add(newOption);
        }
        materialDD.options = options;

        options = new List<Dropdown.OptionData>(landscapes.Count);
        var none = new Dropdown.OptionData("None");
        options.Add(none);
        foreach (var landscape in landscapes)
        {
            var newOption = new Dropdown.OptionData(landscape.name);
            options.Add(newOption);
        }
        landscapeDD.options = options;

        options = new List<Dropdown.OptionData>(props.Count);
        options.Add(none);
        foreach (var prop in props)
        {
            var newOption = new Dropdown.OptionData(prop.name);
            options.Add(newOption);
        }
        propDD.options = options;
        currentProp = null;



        materialDD.onValueChanged.AddListener(delegate {
            MaterialDropdownValueChangedHandler(materialDD);
        });

        skyboxDD.onValueChanged.AddListener(delegate {
            SkyboxDropdownValueChangedHandler(skyboxDD);
        });

        landscapeDD.onValueChanged.AddListener(delegate {
            LandscapeDropdownValueChangedHandler(landscapeDD);
        });

        propDD.onValueChanged.AddListener(delegate {
            PropDropdownValueChangedHandler(propDD);
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ShowPanel();
        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();
    }

	void OnGUI()
    {
        
        _cursor.transform.SetParent(GameObject.Find("Canvas").transform);
        var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y + mouseYoffset, 1.5f));
        var rot = new Quaternion(0, 0, 0, 0);
        _cursor.transform.position = pos;
        _cursor.transform.localRotation = rot;
    }

    void ShowPanel()
    {
        if (mainMenu.GetComponent<PanelFades>().Visible() 
            || folderWindow.GetComponent<PanelFades>().Visible() 
            || sceneWindow.GetComponent<PanelFades>().Visible())
        {
            mainMenu.GetComponent<PanelFades>().FadeOut();
            folderWindow.GetComponent<PanelFades>().FadeOut();
            sceneWindow.GetComponent<PanelFades>().FadeOut();
        }
        else
            mainMenu.GetComponent<PanelFades>().FadeIn();
    }

    void CursorControl()
    {

    }

    public void browseFiles()
    {
        fileButtons.Clear();
        folderWindow.GetComponent<PanelFades>().FadeIn();
        mainMenu.GetComponent<PanelFades>().FadeOut();
        var filesPerPage = 7;
        var path = Application.dataPath + "/Models";
        var filenamesArray = Directory.GetFiles(path);
        var filenames = new List<string>();
        foreach (var fna in filenamesArray)
        {
            if (fna.EndsWith(".STL") || fna.EndsWith(".stl"))
                filenames.Add(fna);
        }
        var numFiles = filenames.Count;
        var numPages = Mathf.CeilToInt(numFiles / filesPerPage);
        for (int pgNum = 0; pgNum < numPages; pgNum++)
        {
            fileButtons.Add(new List<GameObject>());
            for (int i = (pgNum * filesPerPage); i < (pgNum * filesPerPage) + filesPerPage; i ++)
            {
                if (i >= filenames.Count) return;
                var newButton = Instantiate(button) as GameObject;
                newButton.transform.SetParent(folderWindow.transform);
                var pos = new Vector3(0, (-i - (pgNum * filesPerPage - filesPerPage)) * 40 - 70, 0);
                var rot = new Quaternion(0, 0, 0, 0);
                newButton.transform.localPosition = pos;
                newButton.transform.localScale = Vector3.one;
                newButton.transform.localRotation = rot;

                newButton.name = filenames[i];
                newButton.GetComponentInChildren<Text>().text = "  " + Path.GetFileName(filenames[i]);
                fileButtons[pgNum].Add(newButton);
                fileButtons[pgNum][fileButtons[pgNum].IndexOf(newButton)].active = false;
            }
        }
        foreach (var f in fileButtons[0])
        {
            f.active = true;
        }
    }

    public void loadFile(string fileName)
    {
        stlInterpreter.ClearAll();
        var reader = new StreamReader(fileName);

        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            scanSTL(line);
        }
        GameObject.Find("MESH").GetComponent<MakeMesh>().MergeMesh();
    }

    void scanSTL(string _line)
    {
        _line = _line.Trim();
        var chunks = _line.Split(' ');
        if (_line.Contains("outer"))
        {
            currentVertices.Clear();
            stlInterpreter.outerloop();
        }
        else if (_line.Contains("endloop"))
        {
            stlInterpreter.endloop(_line);
        }
        else if (_line.Contains("vertex"))
        {
            stlInterpreter.vertex(_line);
        }

        else if (_line.Contains("normal"))
        {
            stlInterpreter.normal(_line);
        }
    }

    public void EditScene()
    {
        sceneWindow.GetComponent<PanelFades>().FadeIn();
        folderWindow.GetComponent<PanelFades>().FadeOut();
        mainMenu.GetComponent<PanelFades>().FadeOut();
    }

    private void MaterialDropdownValueChangedHandler(Dropdown target)
    {
        MM.ChangeMaterial(target.value);
    }

    private void SkyboxDropdownValueChangedHandler(Dropdown target)
    {
        RenderSettings.skybox = skyboxes[target.value]; 
    }

    private void LandscapeDropdownValueChangedHandler(Dropdown target)
    {
        var tmp = currentLandscape;
        if (target.value == 0)
        {
            currentLandscape = null;
        }
        else
        {
            currentLandscape = landscapes[target.value - 1];
            currentLandscape.active = true;
        }
        if (tmp != null)
            tmp.active = false;
    }

    private void PropDropdownValueChangedHandler(Dropdown target)
    {
        var tmp = currentProp;
        if (target.value == 0)
        {
            currentProp = null;
        }
        else
        {
            currentProp = props[target.value - 1];
            currentProp.active = true;
        }
        if (tmp != null)
            tmp.active = false;
    }
}

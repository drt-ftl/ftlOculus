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
    public Dropdown groundDD;

    private GameObject _cursor;
    private MakeMesh MM;

    public static GameObject mainMenu;
    public static GameObject folderWindow;
    public static GameObject sceneWindow;
    private List<List<GameObject>> fileButtons = new List<List<GameObject>>();
    private StlInterpreter stlInterpreter;
    public static List<Vector3> currentVertices = new List<Vector3>();
    public static List<Vector3> vertices = new List<Vector3>();
    public static Vector3 Min = new Vector3(1000, 1000, 1000);
    public static Vector3 Max = new Vector3(-1000, -1000, -1000);
    public List<Material> skyboxes = new List<Material>();

    void Start ()
    {
        stlInterpreter = new StlInterpreter();
        mainMenu = GameObject.Find("Main Menu");
        folderWindow = GameObject.Find("Folder Window");
        sceneWindow = GameObject.Find("Scene Window");
        var fwPos = Vector3.zero;
        fwPos.y = Screen.height / 2;
        fwPos.z = 2.4f;

        folderWindow.GetComponent<RectTransform>().localPosition = fwPos;
        folderWindow.active = false;
        sceneWindow.GetComponent<RectTransform>().localPosition = fwPos;
        sceneWindow.active = false;
        _cursor = Instantiate(cursor) as GameObject;
        _cursor.transform.SetParent(mainMenu.transform);
        var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
        var rot = new Quaternion(0, 0, 0, 0);
        _cursor.transform.localPosition = pos;
        _cursor.transform.localRotation = rot;
        MM = GameObject.Find("MESH").GetComponent<MakeMesh>();
        MM.material = Mat;
        MM.Begin();

        materialDD.onValueChanged.AddListener(delegate {
            MaterialDropdownValueChangedHandler(materialDD);
        });

        skyboxDD.onValueChanged.AddListener(delegate {
            SkyboxDropdownValueChangedHandler(skyboxDD);
        });

        groundDD.onValueChanged.AddListener(delegate {
            GroundDropdownValueChangedHandler(groundDD);
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ShowPanel();
        if (Input.GetKeyDown(KeyCode.F))
        {
            mainMenu.GetComponent<PanelFades>().FadeOut();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            mainMenu.GetComponent<PanelFades>().FadeIn();
        }
    }

	void OnGUI()
    {
        _cursor.transform.SetParent(GameObject.Find("Canvas").transform);
        var pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.93f));
        var rot = new Quaternion(0, 0, 0, 0);
        _cursor.transform.position = pos;
        _cursor.transform.localRotation = rot;
    }

    void ShowPanel()
    {
        if (mainMenu.active)
            mainMenu.active = false;
        else
            mainMenu.active = true;
    }

    void CursorControl()
    {

    }

    public void browseFiles()
    {
        folderWindow.active = true;
        mainMenu.GetComponent<PanelFades>().FadeOut();
        var filesPerPage = 5;
        var path = Application.dataPath + "/Models";
        var filenames = Directory.GetFiles(path);
        var numFiles = filenames.Length;
        var numPages = Mathf.CeilToInt(numFiles / filesPerPage);
        for (int pgNum = 0; pgNum < numPages; pgNum++)
        {
            fileButtons.Add(new List<GameObject>());
            for (int i = (pgNum * filesPerPage); i < (pgNum * filesPerPage) + filesPerPage; i ++)
            {
                if (i >= filenames.Length) return;
                if (filenames[i].EndsWith(".meta")) continue;
                var newButton = Instantiate(button) as GameObject;
                newButton.transform.SetParent(folderWindow.transform);
                var pos = new Vector3(0, (i - pgNum * filesPerPage) * 30 + 30, 0);
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
        folderWindow.GetComponent<PanelFades>().FadeIn();
    }

    public void loadFile(string fileName)
    {
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
        sceneWindow.active = true;
        mainMenu.GetComponent<PanelFades>().FadeOut();
        sceneWindow.GetComponent<PanelFades>().FadeIn();
    }

    private void MaterialDropdownValueChangedHandler(Dropdown target)
    {
        MM.ChangeMaterial(target.value);
    }

    private void SkyboxDropdownValueChangedHandler(Dropdown target)
    {
        RenderSettings.skybox = skyboxes[target.value];
    }

    private void GroundDropdownValueChangedHandler(Dropdown target)
    {
        MM.ChangeMaterial(target.value);
    }
}

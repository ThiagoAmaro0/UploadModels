using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UploadManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private TMP_InputField _descInput;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Transform previewAnchor;
    [SerializeField] private FirebaseManager firebaseManager;
    [SerializeField] private CameraController camControll;
    [SerializeField] private Sprite defaultSpriteIcon;
    private ModelAsset _currentModel;
    private GameObject preview;
    private Texture2D texture;
    private Texture2D icon;
    private string _modelPath;
    private string _texturePath;
    private string _iconPath;
    public Action loadingAction;
    public Action uploadDoneAction;
    public static UploadManager instance;
    public List<ModelAsset> modelAssets;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png"), new FileBrowser.Filter("3D Models", ".obj"));
        _currentModel = new ModelAsset("", "");
        modelAssets = new List<ModelAsset>();
    }
    private void OnApplicationQuit()
    {
        string[] filePaths = Directory.GetFiles(Application.persistentDataPath); 
        foreach (string filePath in filePaths) 
            File.Delete(filePath);

    }

    public void LoadModelButton()
    {
        FileBrowser.SetDefaultFilter(".obj");
        StartCoroutine(LoadModel());
    }

    public void LoadTextureButton()
    {
        FileBrowser.SetDefaultFilter(".jpg");
        StartCoroutine(LoadTexture());
    }

    public void LoadIconButton()
    {
        FileBrowser.SetDefaultFilter(".jpg");
        StartCoroutine(LoadIcon());
    }

    public void FindButton()
    {
        foreach (ModelAsset model in modelAssets)
        {
            _currentModel = null;
            if (_nameInput.text == model.name)
            {
                _currentModel = model;
                _descInput.text = model.description;
                _descInput.Select();
                break;
            }
        }

        if (_currentModel == null)
            return;

        firebaseManager.LoadModel(_currentModel);
        firebaseManager.LoadTexture(_currentModel);
        firebaseManager.LoadIcon(_currentModel);

        //preview.GetComponent<Renderer>().material.mainTexture = texture;
    }

    public void DeleteButton()
    {
        modelAssets.Remove(_currentModel);
        Destroy(preview);
        _nameInput.text = "";
        _descInput.text = "";
        _iconImage.sprite = defaultSpriteIcon;
        FirebaseManager.instance.DeleteModel(_currentModel);
        
    }

    private IEnumerator LoadModel()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Carregar Arquivos e Pastas", "Carregar");
        if (preview)
            Destroy(preview);
        SetModel(ObjReader.use.ConvertFile(_modelPath, false)[0], FileBrowser.Result[0]);
    }

    private IEnumerator LoadTexture()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Carregar Arquivos e Pastas", "Carregar");
        _texturePath = FileBrowser.Result[0];
        byte[] bytes = File.ReadAllBytes(_texturePath);
        texture = new Texture2D(2, 2);
        texture.LoadImage(bytes);
        if (preview)
        {
            ApplyTexture();
        }
    }

    private IEnumerator LoadIcon()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Carregar Arquivos e Pastas", "Carregar");
        _iconPath = FileBrowser.Result[0];
        byte[] bytes = File.ReadAllBytes(_iconPath);
        icon = new Texture2D(2, 2);
        icon.LoadImage(bytes);
        _iconImage.sprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), Vector2.zero);
    }

    private void ApplyTexture()
    {
        if (texture && preview)
        {
            Renderer renderer = preview.GetComponent<Renderer>();
            foreach (Material material in renderer.materials)
            {
                material.mainTexture = texture;
            }
        }
            
    }

    public void Submit()
    {
        _currentModel.name = _nameInput.text;
        _currentModel.description = _descInput.text;
        //StartCoroutine(PlayfabManager.instance.SetTitleData(currentModel));
        if (modelAssets == null)
            modelAssets = new List<ModelAsset>();
        modelAssets.Add(new ModelAsset(_currentModel.name, _currentModel.description));
        StartCoroutine(FirebaseManager.instance.UploadData(_currentModel));
    }

    public string GetModelPath()
    {
        return _modelPath;
    }

    public string GetTexturePath()
    {
        return _texturePath;
    }

    public string GetIconPath()
    {
        return _iconPath;
    }

    public void SetModel(GameObject _preview, string path)
    {
        _modelPath = path;
        camControll.ResetZoom();
        if (preview)
            Destroy(preview);
        preview = _preview;
        preview.transform.parent = previewAnchor;
        preview.transform.localPosition = new Vector3();
        ApplyTexture();
    }

    public void SetTexture(Texture2D _texture, string path)
    {
        _texturePath = path;
        texture = _texture;
        ApplyTexture();
    }

    public void SetIcon(Texture2D _texture, string path)
    {
        _iconPath = path;
        icon = _texture;
        _iconImage.sprite = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), Vector2.zero);
    }
}

using Firebase.Database;
using Firebase.Extensions;
using Firebase.Storage;
using Michsky.UI.ModernUIPack;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class FirebaseManager : MonoBehaviour
{
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private int _uploads;
    private StorageReference storageReference;
    private FirebaseStorage storage;
    private FirebaseDatabase _dbReference;
    public static FirebaseManager instance;

    private void Awake()
    {
        instance = this;
        storage = FirebaseStorage.DefaultInstance;
        storageReference = storage.GetReferenceFromUrl("gs://meta4chain-ar.appspot.com/");
        _dbReference = FirebaseDatabase.GetInstance("https://meta4chain-ar-default-rtdb.firebaseio.com/");
        GetData();
    }

    private void GetData()
    {
        _dbReference.GetReference("ModelAssets").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                // Handle the error...
                Debug.LogError(task.Exception.Message);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                UploadManager.instance.modelAssets = JsonUtility.FromJson<ModelData>(snapshot.GetRawJsonValue()).modelAssets;

            }
        });
    }

    public IEnumerator UploadData(ModelAsset modelAsset)
    {
        _uploads = 0;
        UploadModel(modelAsset);
        UploadTexture(modelAsset);
        UploadIcon(modelAsset);
        UploadManager.instance.loadingAction?.Invoke();

        yield return new WaitUntil(() => _uploads == 3);
        UploadData();
    }

    public void UploadData()
    {
        ModelData modelAssets = new ModelData();
        modelAssets.modelAssets = UploadManager.instance.modelAssets;

        string json = JsonUtility.ToJson(modelAssets);
        _dbReference.GetReference("ModelAssets").SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                UploadManager.instance.uploadDoneAction?.Invoke();
                Debug.LogError(task.Exception.Message);
            }
            else if (task.IsCompleted)
            {
                UploadManager.instance.uploadDoneAction?.Invoke();
                Debug.Log("Upload Successful!");
            }
        });
    }

    private void UploadTexture(ModelAsset modelAsset)
    {
        print("Start Upload");
        byte[] data = File.ReadAllBytes(UploadManager.instance.GetTexturePath());
        StorageReference uploadRef = storageReference.Child("/Models/" + modelAsset.name + "/" + modelAsset.name + "_Texture.jpeg");
        Debug.Log("File upload started");
        //Editing Metadata
        var newMetadata = new MetadataChange();
        newMetadata.ContentType = "image/jpeg";
        uploadRef.PutBytesAsync(data, newMetadata).ContinueWith((Task<StorageMetadata> task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log(task.Exception.ToString());
                // Uh-oh, an error occurred!
            }
            else
            {
                // Metadata contains file metadata such as size, content-type, and md5hash.
                StorageMetadata metadata = task.Result;
                string md5Hash = metadata.Md5Hash;
                Debug.Log("Finished uploading...");
                Debug.Log("md5 hash = " + md5Hash);
                _uploads++;
                progressBar.ChangeValue(_uploads * (100 / 3));
                progressBar.loadingBar.fillAmount = _uploads * (100 / 3) / 100;
            }
        });

    }

    private void UploadIcon(ModelAsset modelAsset)
    {
        print("Start Upload");
        byte[] data = File.ReadAllBytes(UploadManager.instance.GetIconPath());
        StorageReference uploadRef = storageReference.Child("/Models/" + modelAsset.name + "/" + modelAsset.name + "_Icon.jpeg");
        Debug.Log("File upload started");
        //Editing Metadata
        var newMetadata = new MetadataChange();
        newMetadata.ContentType = "image/jpeg";
        uploadRef.PutBytesAsync(data, newMetadata).ContinueWith((Task<StorageMetadata> task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log(task.Exception.ToString());
                // Uh-oh, an error occurred!
            }
            else
            {
                // Metadata contains file metadata such as size, content-type, and md5hash.
                StorageMetadata metadata = task.Result;
                string md5Hash = metadata.Md5Hash;
                Debug.Log("Finished uploading...");
                Debug.Log("md5 hash = " + md5Hash);
                _uploads++;
                progressBar.ChangeValue(_uploads * (100 / 3));
                progressBar.loadingBar.fillAmount = _uploads * (100 / 3) / 100;
            }
        });

    }

    private void UploadModel(ModelAsset modelAsset)
    {
        print("Start Upload");
        byte[] data = File.ReadAllBytes(UploadManager.instance.GetModelPath());
        Debug.Log("File upload started");
        StorageReference uploadRef = storageReference.Child("/Models/" + modelAsset.name + "/" + modelAsset.name + "_Model.obj");
        var newMetadata = new MetadataChange();
        newMetadata.ContentType = "3D Model/obj";
        uploadRef.PutBytesAsync(data, newMetadata).ContinueWith((Task<StorageMetadata> task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log(task.Exception.ToString());
                // Uh-oh, an error occurred!
            }
            else
            {
                // Metadata contains file metadata such as size, content-type, and md5hash.
                StorageMetadata metadata = task.Result;
                string md5Hash = metadata.Md5Hash;
                Debug.Log("Finished uploading...");
                Debug.Log("md5 hash = " + md5Hash);
                _uploads++;
                progressBar.ChangeValue(_uploads * (100 / 3));
                progressBar.loadingBar.fillAmount = _uploads * (100 / 3) / 100;
            }
        });
    }

    public void LoadModel(ModelAsset model)
    {
        string path = Path.Combine(Application.persistentDataPath, model.name + ".obj");
        if (!File.Exists(path))
        {
                DownloadModel(model);
        }
        else
        {
                UploadManager.instance.SetModel(ObjReader.use.ConvertFile(path, false)[0], path);

        }
    }

    public void LoadTexture(ModelAsset model)
    {
        string path = Path.Combine(Application.persistentDataPath, model.name + ".jpeg");
        if (!File.Exists(path))
        {
            DownloadTexture(model);
        }
        else
        {
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            tex.LoadImage(File.ReadAllBytes(path));
            UploadManager.instance.SetTexture(tex, path);
        }
    }

    public void LoadIcon(ModelAsset model)
    {
        string path = Path.Combine(Application.persistentDataPath, model.name + "_Icon.jpeg");
        if (!File.Exists(path))
        {
            DownloadIcon(model);
        }
        else
        {
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            tex.LoadImage(File.ReadAllBytes(path));
            UploadManager.instance.SetIcon(tex, path);
        }
    }

    private void DownloadModel(ModelAsset model)
    {
        StorageReference pathReference = storage.GetReference($"Models/{model.name}/{model.name}_Model.obj");

        pathReference.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("Download URL: " + task.Result);
                StartCoroutine(DownloadOBJ(task.Result.ToString(), model));
            }
            else
                Debug.LogError(task.Exception);
        });
    }

    private async void DownloadTexture(ModelAsset model)
    {
        StorageReference pathReference = storage.GetReference($"Models/{model.name}/{model.name}_Texture.jpeg");

        await pathReference.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("Download URL: " + task.Result);
                StartCoroutine(DownloadTexture(task.Result.ToString(), model));
            }
            else
                Debug.LogError(task.Exception);
        });
    }

    private async void DownloadIcon(ModelAsset model)
    {
        StorageReference pathReference = storage.GetReference($"Models/{model.name}/{model.name}_Icon.jpeg");

        await pathReference.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log("Download URL: " + task.Result);
                StartCoroutine(DownloadIcon(task.Result.ToString(), model));
            }
            else
                Debug.LogError(task.Exception);
        });
    }

    private IEnumerator DownloadOBJ(string MediaUrl, ModelAsset model)
    {
        print("Start Model Download");
        var www = new WWW(MediaUrl);
        yield return www;
        byte[] bytes = www.bytes;

        string path = Path.Combine(Application.persistentDataPath, model.name + ".obj");
        //File.Create(path);
        File.WriteAllBytes(path, bytes);


        UploadManager.instance.SetModel(ObjReader.use.ConvertFile(path, false)[0], path);

        print("Done");

    }

    private IEnumerator DownloadTexture(string MediaUrl, ModelAsset model)
    {
        yield return StartCoroutine(ImageRequest(MediaUrl, (UnityWebRequest req) =>
        {
            if (req.isNetworkError || req.isHttpError)
            {
                Debug.Log($"{req.error}: {req.downloadHandler.text}");
            }
            else
            {

                string path = Path.Combine(Application.persistentDataPath, model.name + ".jpeg");
                File.WriteAllBytes(path, DownloadHandlerTexture.GetContent(req).EncodeToJPG());
                UploadManager.instance.SetTexture(DownloadHandlerTexture.GetContent(req), path);

                print("Done");
            }
        }));
    }

    private IEnumerator DownloadIcon(string MediaUrl, ModelAsset model)
    {
        yield return StartCoroutine(ImageRequest(MediaUrl, (UnityWebRequest req) =>
        {
            if (req.isNetworkError || req.isHttpError)
            {
                Debug.Log($"{req.error}: {req.downloadHandler.text}");
            }
            else
            {

                string path = Path.Combine(Application.persistentDataPath, model.name + "_Icon.jpeg");
                File.WriteAllBytes(path, DownloadHandlerTexture.GetContent(req).EncodeToJPG());
                UploadManager.instance.SetIcon(DownloadHandlerTexture.GetContent(req), path);

                print("Done");
            }
        }));
    }

    IEnumerator ImageRequest(string url, Action<UnityWebRequest> callback)
    {
        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
        {
            yield return req.SendWebRequest();
            callback(req);
        }
    }

    public void DeleteModel(ModelAsset model)
    {
        DeleteFromDatabase("/Models/" + model.name + "/" + model.name +"_Model.obj");
        DeleteFromDatabase("/Models/" + model.name + "/" + model.name +"_Texture.jpeg");
        DeleteFromDatabase("/Models/" + model.name + "/" + model.name + "_Icon.jpeg");
    }
    private void DeleteFromDatabase(string path)
    {
        StorageReference desertRef = storageReference.Child(path);

        // Delete the file
        desertRef.DeleteAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                Debug.Log("File deleted successfully.");
                UploadData();
            }
            else
            {
                // Uh-oh, an error occurred!
            }
        });
    }


}

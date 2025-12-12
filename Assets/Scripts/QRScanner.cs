using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

using UnityEngine.SceneManagement;
//using TBEasyWebCam;

public class QRScanner : MonoBehaviour
{
    WebCamTexture webcamTexture;
    string QrCode = string.Empty;

    public Text UiText;

    public bool isOpenBrowserIfUrl;

    public RawImage renderer;

    void Start()
    {
    }

    private void OnEnable()
    {
        webcamTexture = new WebCamTexture(512, 512);
        renderer.texture = webcamTexture;
        StartCoroutine(GetQRCode());        
    }

    IEnumerator GetQRCode()
    {
        IBarcodeReader barCodeReader = new BarcodeReader();
        webcamTexture.Play();
        var snap = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.ARGB32, false);
        while (string.IsNullOrEmpty(QrCode))
        {
            try
            {
                snap.SetPixels32(webcamTexture.GetPixels32());
                var Result = barCodeReader.Decode(snap.GetRawTextureData(), webcamTexture.width, webcamTexture.height, RGBLuminanceSource.BitmapFormat.ARGB32);
                if (Result != null)
                {
                    QrCode = Result.Text;
                    if (!string.IsNullOrEmpty(QrCode))
                    {
                        Debug.Log("DECODED TEXT FROM QR: " + QrCode);

                        qrScanFinished(QrCode);

                        break;
                    }
                }
            }
            catch (Exception ex) { Debug.LogWarning(ex.Message); }
            yield return null;
        }
        webcamTexture.Stop();
        gameObject.SetActive(false);
    }

    public void qrScanFinished(string dataText)
    {
        Debug.Log(dataText);

        if (isOpenBrowserIfUrl)
        {
            //if (Utility.CheckIsUrlFormat(dataText))
            {
                if (!dataText.Contains("http://") && !dataText.Contains("https://"))
                {
                    dataText = "http://" + dataText;
                }
                Application.OpenURL(dataText);
            }
        }
        this.UiText.text = dataText;
    }

    private void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 50;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        string text =QrCode;
        GUI.Label(rect, text, style);
    }
}

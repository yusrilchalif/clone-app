using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AuthSceneUI : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI errorText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Login() {

        if (emailInput.text is null || passwordInput is null) {
            errorText.gameObject.SetActive(true);
            errorText.text = "Email atau password tidak boleh kosong!";
        }

        AuthManager.Instance.Login(emailInput.text, passwordInput.text, errorText);
    }

    public void ShowPassword() {
        if (passwordInput.contentType == TMP_InputField.ContentType.Password)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
        }
        passwordInput.ForceLabelUpdate();
    }

}

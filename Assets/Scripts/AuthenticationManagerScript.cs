using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;
using System;

public class AuthenticationManagerScript : MonoBehaviour
{
    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();

            // Check if the user is already signed in
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in anonymously.");
            }
            else
            {
                Debug.Log("Already signed in.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during authentication: {e.Message}");
        }
    }
}

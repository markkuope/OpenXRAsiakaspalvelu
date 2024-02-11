using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class NpcDialog : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject startDialogHelmi;

    [SerializeField] private Transform standingPoint;

    private Transform avatar;

    

    private async void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            avatar = other.transform;

            // disable player input
            avatar.GetComponent<PlayerInput>().enabled = false;

            await Task.Delay(50);

            // teleport the avatar to standing point
            avatar.position = standingPoint.position;
            avatar.rotation = standingPoint.rotation;

            // disable main cam, enable dialog cam
            mainCamera.SetActive(false);
            startDialogHelmi.SetActive(true);

            // d?splay cursor
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Recover()
    {
        avatar.GetComponent<PlayerInput>().enabled = true;

        mainCamera.SetActive(true);
        startDialogHelmi.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void LopetetaanPeli()
    {
        Debug.Log("lopetetaan peli XR ohjaus toimii");

        Application.Quit();
    }

    public void TakaisinAlkuun()
    {
        SceneManager.LoadScene("Level01");
    }

}


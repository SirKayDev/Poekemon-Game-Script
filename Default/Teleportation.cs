using UnityEngine;

public class Teleportation : MonoBehaviour
{
    private GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); // Find the player by tag
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            switch (gameObject.tag)
            {
                case "Door 1":
                    TeleportPlayer(GetTeleportLocation("Door 1"));
                    break;
                case "Door 2":
                    TeleportPlayer(GetTeleportLocation("Door 2"));
                    break;
                case "Door 3":
                    TeleportPlayer(GetTeleportLocation("Door 3"));
                    break;
                case "Door 4":
                    TeleportPlayer(GetTeleportLocation("Door 4"));
                    break;
                case "Door 5":
                    TeleportPlayer(GetTeleportLocation("Door 5"));
                    break;
                case "Door 6":
                    TeleportPlayer(GetTeleportLocation("Door 6"));
                    break;
            }
        }
    }

    private void TeleportPlayer(TeleportLocation location)
    {
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        player.transform.position = location.position;
        player.transform.rotation = Quaternion.Euler(location.rotation);

        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    // This function returns the teleport location and rotation based on the GameObject name (tag).
    private TeleportLocation GetTeleportLocation(string doorName)
    {
        switch (doorName)
        {
            case "Door 1":
                return new TeleportLocation(new Vector3(-58.88f, 3.182f, -698.98f), Vector3.zero);
            case "Door 2":
                return new TeleportLocation(new Vector3(-113.49f, 10.328f, -634.29f), Vector3.zero);
            case "Door 3":
                return new TeleportLocation(new Vector3(153.93f, 5.085f, -790.43f), Vector3.zero);
            case "Door 4":
                return new TeleportLocation(new Vector3(-41.30998f, 3.182f, -751.04f), Vector3.zero);
            case "Door 5":
                return new TeleportLocation(new Vector3(-77.29f, 3.222f, -696.65f), Vector3.zero);
            case "Door 6":
                return new TeleportLocation(new Vector3(-69.33f, 3.222f, -727.55f), Vector3.zero);
            default:
                return new TeleportLocation(Vector3.zero, Vector3.zero);
        }
    }
}

// This class holds the position and rotation information for teleportation
public class TeleportLocation
{
    public Vector3 position;
    public Vector3 rotation;

    public TeleportLocation(Vector3 pos, Vector3 rot)
    {
        position = pos;
        rotation = rot;
    }
}


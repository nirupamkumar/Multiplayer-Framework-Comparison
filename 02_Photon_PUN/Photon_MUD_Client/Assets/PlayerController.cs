using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    public float health = 100f;
    public float attack = 10f;
    public float speed = 5f;

    private Vector3 direction;
    private bool isMoving = false;

    private Quaternion targetRotation;

    void Update()
    {
        if (photonView.IsMine)
        {
            ProcessInput();
        }
    }

    void ProcessInput()
    {
        if (isMoving) 
            return;

        direction = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.W))
        {
            direction = Vector3.up;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            direction = Vector3.down;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            direction = Vector3.left;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            direction = Vector3.right;
        }

        if (direction != Vector3.zero)
        {
            TryMovePlayer(direction);
        }
    }

    void TryMovePlayer(Vector3 dir)
    {
        Vector3 targetPosition = transform.position + dir;

        // Check if the target position is within the world grid
        MapLegend tileType = WorldManager.GetTileTypeAtPosition(targetPosition);

        // Only allow movement onto valid tiles
        if (tileType == MapLegend.Tile || tileType == MapLegend.Health || tileType == MapLegend.Attack || tileType == MapLegend.Speed)
        {
            // Handle pickups
            HandleTileEffect(tileType);
            RotatePlayer(dir);
            MovePlayer(targetPosition);
        }
        else
        {
            // Cannot move onto this tile
            Debug.Log("Cannot move onto this tile.");
        }
    }

    void RotatePlayer(Vector3 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        targetRotation = Quaternion.Euler(0f, 0f, angle - 90f); 

        transform.rotation = targetRotation;
    }

    void MovePlayer(Vector3 targetPosition)
    {
        isMoving = true;
        StartCoroutine(LerpPosition(targetPosition, 0.2f));
    }

    System.Collections.IEnumerator LerpPosition(Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            // Optionally, smooth the rotation
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
        isMoving = false;
    }

    void HandleTileEffect(MapLegend tileType)
    {
        if (tileType == MapLegend.Health)
        {
            health += 50f;
            Debug.Log("Picked up Health!");
        }
        else if (tileType == MapLegend.Attack)
        {
            attack += 10f;
            Debug.Log("Picked up Attack!");
        }
        else if (tileType == MapLegend.Speed)
        {
            speed += 5f;
            Debug.Log("Picked up Speed!");
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Sync player data
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(health);
            stream.SendNext(attack);
            stream.SendNext(speed);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
            health = (float)stream.ReceiveNext();
            attack = (float)stream.ReceiveNext();
            speed = (float)stream.ReceiveNext();
        }
    }
}

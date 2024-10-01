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
        MapLegend tileType = WorldManager.GetTileTypeAtPosition(targetPosition);

        if (tileType == MapLegend.Tile || tileType == MapLegend.Health || tileType == MapLegend.Attack || tileType == MapLegend.Speed)
        {
            HandleTileEffect(tileType);
            RotatePlayer(dir);
            MovePlayer(targetPosition);
        }
        else
        {
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
        CustomLogger.Instance.Log("Player " + PhotonNetwork.NickName + " moving from " + transform.position + " to " + targetPosition);
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
            CustomLogger.Instance.Log("Player " + PhotonNetwork.NickName + " picked up Health. New Health: " + health);
            Debug.Log("Picked up Health!");
        }
        else if (tileType == MapLegend.Attack)
        {
            attack += 10f;
            CustomLogger.Instance.Log("Player " + PhotonNetwork.NickName + " picked up Attack. New Attack: " + attack);
            Debug.Log("Picked up Attack!");
        }
        else if (tileType == MapLegend.Speed)
        {
            speed += 5f;
            CustomLogger.Instance.Log("Player " + PhotonNetwork.NickName + " picked up Speed. New Speed: " + speed);
            Debug.Log("Picked up Speed!");
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
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

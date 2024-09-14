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

    void Update()
    {
        if (photonView.IsMine)
        {
            ProcessInput();
        }
    }

    void ProcessInput()
    {
        if (isMoving) return;

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
            MovePlayer(direction);
        }
    }

    void MovePlayer(Vector3 dir)
    {
        isMoving = true;
        Vector3 targetPosition = transform.position + dir;
        StartCoroutine(LerpPosition(targetPosition, 0.2f));
    }

    System.Collections.IEnumerator LerpPosition(Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = transform.position;

        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Sync player data
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(health);
            stream.SendNext(attack);
            stream.SendNext(speed);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            health = (float)stream.ReceiveNext();
            attack = (float)stream.ReceiveNext();
            speed = (float)stream.ReceiveNext();
        }
    }
}

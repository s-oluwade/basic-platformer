using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraAreaSwitcher : MonoBehaviour
{
    [SerializeField] CinemachineConfiner confiner;
    [SerializeField] private PolygonCollider2D thisArea;
    [SerializeField] private PolygonCollider2D exit1;
    [SerializeField] private PolygonCollider2D exit2;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            confiner.m_BoundingShape2D = thisArea;

            // DO SOMETHING WHEN THE PLAYER ENTERS THE SCENE
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (exit1 != null && collision.IsTouching(exit1))
        {
            confiner.m_BoundingShape2D = exit1;
        }
        else if (exit2 != null && collision.IsTouching(exit2))
        {
            confiner.m_BoundingShape2D = exit2;
        }
    }
}

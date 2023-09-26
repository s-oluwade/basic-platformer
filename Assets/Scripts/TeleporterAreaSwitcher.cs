using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleporterAreaSwitcher : MonoBehaviour
{
    //public enum Locations { redZone, greenZone, blueZone }

    private int currentTeleporter;
    private int thisTeleporterIndex;
    private bool teleporterOn;
    private bool playerEntered;
    private GameObject target;
    private GameObject[] teleporters;
    private CinemachineVirtualCamera vcam;
    [SerializeField] CinemachineConfiner confiner;
    //[SerializeField] public Locations location;
    [SerializeField] public PolygonCollider2D thisZone;
    [SerializeField] private PolygonCollider2D allZones;

    // Start is called before the first frame update
    void Start()
    {
        vcam = GameObject.FindWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        teleporters = GameObject.FindGameObjectsWithTag("Teleporter");
        target = gameObject;             // Set default to bypass unassigned check
        teleporterOn = false;
        playerEntered = false;

        for (int i = 0; i < teleporters.Length - 1; i++)
        {
            int min = i;
            for (int j = 1; j < teleporters.Length; j++)
            {
                float teleporter_y = teleporters[j].transform.position.y;
                if (teleporter_y < teleporters[min].transform.position.y)
                {
                    min = j;
                }
            }

            if (min != i)
            {
                GameObject temp = teleporters[min];
                teleporters[min] = teleporters[i];
                teleporters[i] = temp;
            }
        }

        thisTeleporterIndex = teleporters.Length;

        for (int i = 0; i < teleporters.Length; i++)
        {
            if (string.Compare(teleporters[i].name, name) == 0)
            {

                thisTeleporterIndex = i;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerEntered && !teleporterOn)
        {
            if (Input.GetKey(KeyCode.Z) || Input.GetButton("X"))
            {
                StartCoroutine(runTeleporter());
            }

            else if (vcam.m_Lens.OrthographicSize > 6.0f)
            {
                vcam.m_Lens.OrthographicSize -= 0.05f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerEntered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerEntered = false;
        }
    }

    private IEnumerator runTeleporter()
    {
        teleporterOn = true;              // To indicate that couroutine is already running
        currentTeleporter = thisTeleporterIndex;
        while (Input.GetKey(KeyCode.Z) || Input.GetButton("X"))
        {
            // Up
            if (Input.GetAxisRaw("Vertical") > 0.4f)
            {
                if (currentTeleporter + 1 < teleporters.Length)
                {
                    currentTeleporter = currentTeleporter + 1;
                    target = teleporters[currentTeleporter];
                }
            }

            // Down
            else if (Input.GetAxisRaw("Vertical") < -0.4f)
            {
                if (currentTeleporter - 1 >= 0)
                {
                    currentTeleporter = currentTeleporter - 1;
                    target = teleporters[currentTeleporter];
                }
            }

            confiner.m_BoundingShape2D = allZones;
            vcam.Follow = target.transform;
            vcam.m_Lens.OrthographicSize = 10;

            yield return new WaitForSeconds(0.10f);
        }
        teleporterOn = false;           // Couroutine is off
        target = GameObject.FindGameObjectWithTag("Player");
        target.transform.position = teleporters[currentTeleporter].transform.position;
        confiner.m_BoundingShape2D = teleporters[currentTeleporter].GetComponent<TeleporterAreaSwitcher>().thisZone;

        vcam.Follow = target.transform;
        
        StopCoroutine(runTeleporter());
    }
}

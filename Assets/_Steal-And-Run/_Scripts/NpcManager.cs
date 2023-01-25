using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class NpcManager : MonoBehaviour
{
    [SerializeField] private float valueToSlider = 10;

    [SerializeField] private PlayerController _playerController;

    public PlayerController PlayerController => _playerController;
    private MoneyHolder _moneyHolder;
    
    private AudioSource _audioSource;
    private NpcController[] _npcs;
    public NpcController[] Npcs => _npcs;

    public int NpcDead { get; set; }

    public float ValueToSlider => valueToSlider;
    
    [SerializeField] private GameObject money, hitParticle, emojiCanva;

    [SerializeField] private float speed;

    public float Speed => speed;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        GameManager.Instance.NpcManager = this;
        _moneyHolder = _playerController.MoneyHolder;
        _npcs = GetComponentsInChildren<NpcController>();
    }

    [SerializeField] private float valuetoAdd;

    /// <summary>
    /// Es llamado cuando robas al NpcZ
    /// </summary>
    /// <param name="posNpc"> la posicion para instanciar fx</param>
    /// <param name="cashToAdd">to add for Ui</param>
    /// <param name="objStolen"></param>
    public void WhenNpcStolen(Transform posNpc, float cashToAdd, Transform objStolen)
    {
        Taptic.Light();
        var position = posNpc.position;
        Instantiate(hitParticle, position + Vector3.up * .5f, hitParticle.transform.rotation);
        //var newMoney = Instantiate(money, position + Vector3.up * .5f, money.transform.rotation);
        //_moneyHolder.AddNewItemInHolder(newMoney.transform);
        //_playerController.AddShapesToBag(valuetoAdd);
        //if (objStolen != null) StartCoroutine(_playerController.MoneyHolder.AddNewItemToCollector(objStolen));
        GameManager.Instance.AddCash(cashToAdd);
    }

    /// <summary>
    /// Usado para darles nueva vvelocidad a todos los NPCs que SI te persiguenn
    /// </summary>
    /// <param name="newSpeed">velocidad que dar</param>
    /// <param name="disableNav">si quieres deshabilitar el seguimiento</param>
    public void SetSpeedOfAllNpcs(float newSpeed, bool disableNav)
    {
        foreach (var t in _npcs)
        {
            t.Speed = newSpeed;
            t.TryGetComponent(out NavMeshAgent navMesh);
            if (disableNav && navMesh) navMesh.enabled = false;
        }
    }

    public void AddItemToBagValue() => _playerController.AddShapesToBag(true, valuetoAdd);
    public void LessToBagValue() => _playerController.AddShapesToBag(false, valuetoAdd);
    public void PlaySoundRobbery() => _audioSource.Play();
}

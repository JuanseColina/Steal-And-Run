using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SocialPlatforms;
using Random = UnityEngine.Random;

public class NpcController : MonoBehaviour
{
    private NpcManager _npcManager;
    [SerializeField] private Animator _animator;
    private NavMeshAgent _navMeshAgent;
    private BoxCollider _boxCollider;

    [SerializeField] private bool isPolice;
    enum NpcState
    {
        Walk,
        Idle,
        Running,
        Thinking,
        Talking,
        WalkAndText,
        Text
    }

    public float Speed { get; set; }

    private int anim;
    
    [SerializeField] private NpcState _npcState;


    private bool isAngry;

    private Transform playerPos;


    [SerializeField] private float cash = 10;

    [SerializeField] private GameObject[] objectToRobbery;

    #region AnimationsName

    private string mode = "Mode";
    private string deadNro = "Ndead";
    private string left = "Left"; 
    private string right = "Right";
    #endregion

    [SerializeField] private GameObject emojiCanva;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _boxCollider = GetComponent<BoxCollider>();
    }

    private void Start()
    {
        _npcManager = FindObjectOfType<NpcManager>();
        NpcSelector();
        
    }

    private void Update()
    {
        float time = Time.deltaTime;

        if (_navMeshAgent.enabled)
        {
            _navMeshAgent.SetDestination(playerPos.position);
            _navMeshAgent.speed = _npcManager.PlayerController.MSpeed + 1;
        }
        Transform transform2;
        (transform2 = transform).Translate(new Vector3(0,0, time * Speed));
        var transform1 = _animator.transform;
        transform1.position = transform2.position;
        transform1.rotation = transform2.rotation;
    }

    private void NpcSelector()
    {
        switch (_npcState)
        {
            case NpcState.Walk: anim = 0; 
                Speed = 1.5f;
                break;
            case NpcState.Idle: anim = 1;
                Speed = 0;
                break;
            case NpcState.Running: anim = 2;
                Speed = 3;
                break;
            case NpcState.Talking: anim = 3;
                Speed = 0;
                break;
            case NpcState.Thinking: anim = 4;
                Speed = 0;
                break;
            case NpcState.WalkAndText: anim = 6;
                Speed = 1.5f;
                break;
            case NpcState.Text: anim = 5;
                Speed = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        _animator.SetInteger(mode, anim);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isAngry == false && other.CompareTag("Right") || other.CompareTag("Left"))
        {
            isAngry = true;
            var playerController = other.GetComponentInParent<PlayerController>();
            playerPos = other.transform;
            _npcManager.NpcDead++;
            _npcManager.PlaySoundRobbery();
            if (other.CompareTag("Right"))
            {
                playerController.Punch(right);
            }

            if (other.CompareTag("Left"))
            {
                playerController.Punch(left);
            }

            if (isPolice)
            {
                StartCoroutine(IsNpcAngry());
            }

            if (objectToRobbery != null)
            {
                StolenObject();
            }
            NpcDeadMode(true);
        }
    }

    public void NpcDeadMode(bool toSlider)
    {
        if (isAngry)
        {
            GetComponent<BoxCollider>().enabled = false;
            _npcManager.WhenNpcStolen(transform, cash , StolenObjectToHolder());
            if (toSlider)
            {
                GameManager.Instance.SliderValueAdd(_npcManager.ValueToSlider);
            }
            int randomN = Random.Range(1, 4);
            _animator.SetInteger(deadNro, randomN);
            _animator.SetInteger(mode, 100);
            Speed = 0;
            _navMeshAgent.enabled = false;
        }
    }

    public float speedAnim = 1.4f;
    IEnumerator IsNpcAngry()
    {
        yield return new WaitForSeconds(.75f);
        _animator.SetInteger(deadNro, 100 );
        _npcState = NpcState.Running;
        _animator.speed = speedAnim;
        NpcSelector();
        _navMeshAgent.enabled = true;
        _boxCollider.enabled = true;
        StartCoroutine(SpawnEmojiCanva());
    }
    
    IEnumerator SpawnEmojiCanva()
    {
        yield return new WaitForSeconds(2f);
        emojiCanva.SetActive(true);
        yield return new WaitForSeconds(1f);
        emojiCanva.SetActive(false);
    }

    private void StolenObject()
    {
        foreach (var obj in objectToRobbery)
        {
            obj.transform.SetParent(null);
            obj.GetComponent<BoxCollider>().isTrigger = false;
            obj.GetComponent<Rigidbody>().isKinematic = false;
            StartCoroutine(MoveObjectsToBag(obj));
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    IEnumerator MoveObjectsToBag(GameObject objectToTween)
    {
        yield return new WaitForSeconds(.55f);
        objectToTween.GetComponent<Rigidbody>().useGravity = false;
        objectToTween.GetComponent<BoxCollider>().isTrigger = true;
        objectToTween.GetComponent<ObjectSettings>().follow1 = true;
        // LeanTween.move(objectToTween, playerPos.position + new Vector3(0,0,3), .5f).setOnComplete((() =>
        // {
        //     _npcManager.AddItemToBagValue();
        //     objectToTween.SetActive((false));
        // }));
    }

    private Transform StolenObjectToHolder()
    {
        if (objectToRobbery.Length > 0 )
        {
            return objectToRobbery[Random.Range(0, objectToRobbery.Length - 1)].transform;
        }
        else
        {
            return null;
        }
    }
}

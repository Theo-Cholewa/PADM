using UnityEngine;

public class RessourceTime : MonoBehaviour
{
    public AudioSource SeaSounds;
    public AudioSource BattleSounds;

    [HideInInspector] public int ReadyToFightCount=0;

    PartyTools.UniqueRole Role;

    void Start()
    {
        Role = new(Party.current, this, "ressource_time", null, ()=>General.Reset(), ()=>General.Reset());
    }

    void OnDestroy()
    {
        Role.Dispose();
    }

    void FixedUpdate()
    {
        RessourceClient.current.GoToGoodScene();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.B))
        {
            Debug.Log("ASk for fight");
            RessourceClient.current.Get(Team.RED).AskForFight();
        }
        if(ReadyToFightCount>0 && BattleSounds.mute)
        {
            BattleSounds.mute = false;
        }
    }
}

using System;
using System.Data.Common;
using UnityEngine;
public class Ship : MonoBehaviour
{
    public TeamEnum TeamId;
    public float speed = 0f;
    private float waterLevel = 0f;
    public new Renderer renderer;

    public RessourceClient.TeamClient ressources;
    

    void Start()
    {
        ressources = RessourceClient.current.Get(Team.Of(TeamId));
    }

    private int DamageCounter = 0;

    public void ChangeHealth(int offset)
    {
        ressources.Add(RessourceType.Health, offset);
    }

    void FixedUpdate()
    {
        if (speed > 0f) waterLevel += speed / 10000f;
        else waterLevel -= 4f / 10000f;

        if (waterLevel < 0f) waterLevel = 0f;

        Color c = renderer.material.color;
        c.a = 1f - waterLevel;
        renderer.material.color = c;

        // Diminue les vies si le bâteau est en train de couler.
        if (waterLevel > 0f)
        {
            DamageCounter++;
            if (DamageCounter > 50)
            {
                DamageCounter = 0;
                var damageDone = Math.Max(1,(int)(waterLevel*10));
                ressources.Add(RessourceType.Health,-damageDone);
            }
        }
        else
        {
            DamageCounter=0;
        }
    }
}

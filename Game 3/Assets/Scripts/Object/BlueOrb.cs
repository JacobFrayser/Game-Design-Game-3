using UnityEngine;

public class BlueOrb : MonoBehaviour
{
    [Header("General")]
    public AudioClip collectSound;
    [Tooltip("If true, orb is destroyed on collection, otherwise respawns on a timer" + "Default is false")]
    public bool destroyOnCollect = false;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMotor motor = collision.GetComponentInChildren<PlayerMotor>();
        
        if (motor == null)
        {
            return;
        }

        motor.RefreshPulseCharge();
        Debug.Log("collected orb");

        SoundManager.Instance.PlaySound(collectSound);

        if (destroyOnCollect)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}

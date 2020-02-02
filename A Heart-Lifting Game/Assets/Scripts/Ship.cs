﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PieceTypes;
public class Ship : MonoBehaviour
{
    public GameObject explosion;
    public GameObject shot_prefab;
    bool can_shoot = true;
    GameController controller;
    public Vector3 thruster_strength = new Vector3(0,0,0);
    bool moving = false;
    public float max_speed = 10;
    public float acceleration = 1.5f;
    public float rotate_speed = 25;
    float speed = 1.0f;
    Vector2 rotation;
    public GameObject l_thruster;
    public GameObject c_thruster;
    public GameObject r_thruster;
    public GameObject l_tank;
    public GameObject c_tank;
    public GameObject r_tank;
    public float l_tank_left = 0;
    public float c_tank_left = 0;
    public float r_tank_left = 0;
    public float main_depletion = 1.0f;
    public float side_depletion = 1.0f;
    ShipSound sound;
    public GameObject main_thruster;
    public GameObject left_thruster;
    public GameObject right_thruster;
    bool l_thruster_on = false;
    bool r_thruster_on = false;
    int num_shots = 0;
    public int starting_shots = 5;
    public float shot_cooldown = 1;
    public float shot_speed = 25;
    public float shot_duration = 2;
    int num_lives;
    private void Start()
    {
        controller = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        sound = GetComponent<ShipSound>();
    }

    public void Launch()
    {
        moving = true;
        sound.PlayThruster();
        if (c_tank_left > 0)
        {
            foreach (Transform child in main_thruster.transform)
            {
                child.GetComponent<ParticleSystem>().Play();
            }
        }
    }
    public void Land() => moving = false;
    public bool Moving()
    {
        return moving;
    }


    private void Update()
    {
        if (moving)
        {
            if (c_tank_left > 0)
            {
                transform.position += (transform.up * speed) * Time.deltaTime;
                c_tank_left -= main_depletion * Time.deltaTime;
                if (speed < max_speed)
                {
                    speed += acceleration * Time.deltaTime;
                }
            }
            else
            {
                sound.PlayDepleted();
                controller.GameOver(4);
            }
        }

        if (moving)
        {
            if (Input.GetKey("left") && r_tank_left > 0)
            {
                transform.Rotate(Vector3.forward * thruster_strength.x * Time.deltaTime);
                sound.RTurnOn();
                r_tank_left -= side_depletion * Time.deltaTime;
                if (!r_thruster_on)
                {
                    r_thruster_on = true;
                    foreach (Transform child in right_thruster.transform)
                    {
                        child.GetComponent<ParticleSystem>().Play();
                    }
                }
            }
            else
            {
                sound.RTurnOff();
                if (r_thruster_on)
                {
                    r_thruster_on = false;
                    foreach (Transform child in right_thruster.transform)
                    {
                        child.GetComponent<ParticleSystem>().Stop();
                    }
                }
            }
            if (Input.GetKey("right") && l_tank_left > 0)
            {
                transform.Rotate(-Vector3.forward * thruster_strength.z * Time.deltaTime);
                sound.LTurnOn();
                l_tank_left -= side_depletion * Time.deltaTime;
                if (!l_thruster_on)
                {
                    l_thruster_on = true;
                    foreach (Transform child in left_thruster.transform)
                    {
                        child.GetComponent<ParticleSystem>().Play();
                    }
                }
            }
            else
            {
                sound.LTurnOff();
                if (l_thruster_on)
                {
                    l_thruster_on = false;
                    foreach (Transform child in left_thruster.transform)
                    {
                        child.GetComponent<ParticleSystem>().Stop();
                    }
                }
            }

            
        }
        if (Input.GetKey("space"))
        {
            FireLaser();
        }
    }

    void FireLaser()
    {
        if (num_shots > 0 && can_shoot)
        {
            can_shoot = false;
            num_shots--;
            GameObject laser = Instantiate(shot_prefab, new Vector3(transform.position.x + 0.72f, transform.position.y + 2.7f, 0), Quaternion.identity);
            laser.GetComponent<Laser>().StartShot(shot_duration, shot_speed, transform.up);
            StartCoroutine(ShotCooldown());
        }
    }

    IEnumerator ShotCooldown()
    {
        yield return new WaitForSeconds(shot_cooldown);
        can_shoot = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Floor" || collision.gameObject.tag == "Meteor")
        {
            if (collision.gameObject.tag == "Meteor" && num_lives > 0)
            {
                num_lives--;
                // Destroy shield plate component
            }
            else
            {
                if (collision.gameObject.tag == "Meteor")
                {
                    Destroy(collision.gameObject);
                }
                Instantiate(explosion, new Vector3(this.transform.position.x, this.transform.position.y, 0), Quaternion.identity);
                controller.GameOver(3);
                Destroy(this.gameObject);
            }
        }
    }

    public void AttachPiece(Parts part, GameObject obj)
    {
        sound.PlayPartPlace();
        if (part == Parts.L_thruster)
        {
            l_thruster = obj;
            thruster_strength.z = rotate_speed;
            l_tank_left += 50;
        }
        if (part == Parts.C_thruster)
        {
            c_thruster = obj;
            thruster_strength.y = rotate_speed;
            c_tank_left += 100;
        }
        if (part == Parts.R_thruster)
        {
            r_thruster = obj;
            thruster_strength.x = rotate_speed;
            r_tank_left += 50;
        }
        if (part == Parts.L_tank)
        {
            l_tank = obj;
            l_tank_left += 50;
        }
        if (part == Parts.C_tank)
        {
            c_tank = obj;
            c_tank_left += 100;
        }
        if (part == Parts.R_tank)
        {
            r_tank = obj;
            r_tank_left += 50;
        }
        if (part == Parts.L_shield || part == Parts.R_shield)
        {
            num_lives++;
        }
        if (part == Parts.laser)
        {
            num_shots = starting_shots;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Toolkit;

namespace Lab
{
    using SharpDX.Toolkit.Graphics;
    using SharpDX.Toolkit.Input;
    using System.Diagnostics;
    // Player class.
    public class Player : GameObject
    {
        private float sqrt_two = (float) Math.Sqrt(2);
        public int MS = 90;
        private const float SENSITIVITY = 0.03f;
        private Boolean devMode = false;
        private int ENEMY_NEAR_DISTANCE = 80;

        // player position and eye target
        public Vector3 pos;
        public Vector3 target;

        // direction x, y, z axis of the player
        private Vector3 XAxis;
        private Vector3 YAxis; // TODO kyknya ga penting
        private Vector3 ZAxis;
        private float dz,dx;

        public Vector3 velocity;
        public float hp;
        public float damage = 20;

        public SoundEffect enemySoundEffect;
        public Boolean moveForward = false;
        public Boolean moveBackward = false;

        public Player(LabGame game)
        {
            this.pos = new Vector3(16,10,0);
            this.target = new Vector3(16, 12, 10);
            this.XAxis = Vector3.UnitX;
            this.YAxis = Vector3.UnitY;
            this.ZAxis = Vector3.UnitZ;
            this.game = game;
            type = GameObjectType.Player;
            this.hp = 100;

            enemySoundEffect = new SoundEffect(@"Content\UFO.wav", true);
        }

        // Frame update.
        public override void Update(GameTime gameTime)
        {
            if (devMode) {
                var time = (float)gameTime.ElapsedGameTime.TotalSeconds;
                //TODO bad coding removing fire
                if (game.keyboardState.IsKeyDown(Keys.Space)) {  } 

                // Determine velocity based on keys being pressed.
                dz = 0; 
                dx = 0;
                if (game.keyboardState.IsKeyDown(Keys.W)) {
                    dz += 1;
                }
                if (game.keyboardState.IsKeyDown(Keys.S))
                {
                    dz -= 1;
                }
                if (game.keyboardState.IsKeyDown(Keys.D))
                {
                    dx += 1;
                }
                if (game.keyboardState.IsKeyDown(Keys.A)) {
                    dx -= 1;
                }

                if ((dz != 0) && (dx != 0))
                {
                    dz /= sqrt_two;
                    dx /= sqrt_two;
                }

                // Update pos and target based on velocity
                Vector3 change = (dx * MS * time * XAxis) + (dz * MS * time * ZAxis);
                pos += change;
                target += change;

                // Change direction player is facing
                float dRotationY = 0, dRotationX = 0;
                if (game.keyboardState.IsKeyDown(Keys.Left)) {
                    dRotationY--;
                }
                if (game.keyboardState.IsKeyDown(Keys.Right))
                {
                    dRotationY++;
                }
                if (game.keyboardState.IsKeyDown(Keys.Up))
                {
                    dRotationX--;
                }
                if (game.keyboardState.IsKeyDown(Keys.Down))
                {
                    dRotationX++;
                }
                
                Matrix rotationMatrix = Matrix.RotationAxis(YAxis, dRotationY*SENSITIVITY) * Matrix.RotationAxis(XAxis, dRotationX*SENSITIVITY);
                target = Vector3.TransformCoordinate(target - pos, rotationMatrix) + pos; // rotation of target about pos
                XAxis = Vector3.TransformCoordinate(XAxis, rotationMatrix);
                //YAxis = Vector3.TransformCoordinate(YAxis, rotationMatrix); // TODO kyknya ga perlu
                ZAxis = Vector3.TransformCoordinate(ZAxis, rotationMatrix);

                // TODO
                // Keep within the boundaries.
                if (pos.X < game.boundaryLeft) { }
                if (pos.X > game.boundaryRight) {  }

                //basicEffect.World = Matrix.Translation(pos);
            } else {
                var time = (float)gameTime.ElapsedGameTime.TotalSeconds;
                //TODO bad coding removing fire
                if (game.keyboardState.IsKeyDown(Keys.Space)) {  } 

                // Determine velocity based on keys being pressed.
                dz = 0;
                dx = 0;

                if (game.keyboardState.IsKeyDown(Keys.W) || game.mainPage.rightButton.IsPressed)
                {
                    dz += 1;
                }
                if (game.keyboardState.IsKeyDown(Keys.S) || game.mainPage.leftButton.IsPressed)
                {
                    dz -= 1;
                }
                //if (game.keyboardState.IsKeyDown(Keys.D))
                //{
                //    dx += 1;
                //}
                //if (game.keyboardState.IsKeyDown(Keys.A)) {
                //    dx -= 1;
                //}
                
                if ((dz != 0) && (dx != 0))
                {
                    dz /= sqrt_two;
                    dx /= sqrt_two;
                }

                // Update pos and target based on velocity
                velocity = (dx * MS * time * XAxis) + (dz * MS * time * ZAxis);
                if ((pos.X + velocity.X >= 0) && (pos.X + velocity.X <= this.game.landscape.getWidth()))
                {
                    pos.X += velocity.X;
                    target.X += velocity.X;
                }
                if ((pos.Z + velocity.Z >= 0) && (pos.Z + velocity.Z <= this.game.landscape.getWidth()))
                {
                    pos.Z += velocity.Z;
                    target.Z += velocity.Z;
                }

                // Change direction player is facing
                float dRotationY = 0, dRotationX = 0;
                if (game.keyboardState.IsKeyDown(Keys.Left)) {
                    dRotationY--;
                }
                if (game.keyboardState.IsKeyDown(Keys.Right))
                {
                    dRotationY++;
                }
                //if (game.keyboardState.IsKeyDown(Keys.Up))
                //{
                //    dRotationX--;
                //}
                //if (game.keyboardState.IsKeyDown(Keys.Down))
                //{
                //    dRotationX++;
                //}
                try
                {
                    if (Math.Abs(game.accelerometerReading.AccelerationX) > 0.15)
                    {
                        dRotationY += (float)game.accelerometerReading.AccelerationX * 5;
                    }
                }
                catch (Exception)
                {

                }
                Matrix rotationMatrixY = Matrix.RotationAxis(YAxis, dRotationY * SENSITIVITY);
                Vector3 eyeDirection = Vector3.TransformCoordinate(target - pos, rotationMatrixY);
                target = eyeDirection + pos; // rotation of target about pos
                XAxis = Vector3.TransformCoordinate(XAxis, rotationMatrixY);
                //YAxis = Vector3.TransformCoordinate(YAxis, rotationMatrix); // TODO kyknya ga perlu
                ZAxis = Vector3.TransformCoordinate(ZAxis, rotationMatrixY);


                // check collision with lights
                bool isHit = false;
                bool isEnemyNear = false;
                float enemyDistance;
                float minDistance=0;
                bool firstTime = true;
                for (int i = 0; i < game.enemies.Count; i++)
                {
                    enemyDistance = horizontalDistance(this.pos, game.enemies[i].pos);

                    if (firstTime) { minDistance = enemyDistance; firstTime = false; }
                    else if (enemyDistance < minDistance) { minDistance = enemyDistance; }

                    if (enemyDistance < game.enemies[i].pos.Y)
                    {
                        isHit = true;
                        hp -= damage * time;
                        isEnemyNear = true;
                    }
                    else if (enemyDistance < ENEMY_NEAR_DISTANCE)
                    {
                        isEnemyNear = true;
                    }
                }
                game.mainPage.IsHit(isHit);

                // volume
                minDistance = minDistance*minDistance/1000;
                //if minDistance < 
                if (minDistance < 1 || isHit) { minDistance = 1.0f; }
                enemySoundEffect.SetVolume(1 / minDistance);
                if (!enemySoundEffect.isStarted)
                {
                    enemySoundEffect.Play();
                }

                //if (isEnemyNear && !enemySoundEffect.isStarted)
                //{
                //    enemySoundEffect.SetVolume(1 / minDistance);
                //    enemySoundEffect.Play();
                //}
                //else if (!isEnemyNear && enemySoundEffect.isStarted)
                //{
                //    enemySoundEffect.Stop();
                //}


                // TODO
                // Keep within the boundaries.
                if (pos.X < game.boundaryLeft) { }
                if (pos.X > game.boundaryRight) {  }

                //basicEffect.World = Matrix.Translation(pos);
            }
        }

        private float horizontalDistance(Vector3 a, Vector3 b)
        {
            return Vector2.Distance(new Vector2(a.X, a.Z), new Vector2(b.X, b.Z));
        }

        public override void Draw(GameTime gameTime,Effect effect)
        {

        }

        public void move()
        {
            dz += 1;
        }

        public void goBack()
        {
            dz -= 1;
        }

        // React to getting hit by an enemy bullet.
  
    }
}
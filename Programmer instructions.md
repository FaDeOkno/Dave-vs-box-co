# Collaborating Instructions
So just so you know, because I am kindof new to unity so hopefully this will sum up some stuff to make sure everything works.

If there's something that isn't clear or missing just make an issue post on GitHub and ill update this.

## Making walls
As one of the mechanics of the game is grabbing onto walls and jumping, there are specific ways to ensure that the Player movement script knows to perform the action.

First ensure the the wall has a collider, and please mark the object with the "Walls" layer.

Once that is done, you must also mark the wall with a tag, either "LWall" or "Real" depending whether the wall is on the Left or Right.

If setup correctly the player should be able to grab onto walls and jump.

## Physics Objects
To make a physics object be affected by the magnet, it's simple. Just mark it with the "push and pull" layer and give it a RigidBody2D.

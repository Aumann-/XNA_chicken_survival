using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BoundaryClass
{

    public class Boundary
    {

        private static BoundingBox billboardBox;       // boundingbox for the billboard
        private static BoundingBox[] TreeBoxes = new BoundingBox[100];         // array to hold the boundingbox for each tree
        private static BoundingSphere[] treesphere = new BoundingSphere[100];
        private static BoundingSphere[] boundSphere = new BoundingSphere[2];    // array to hold the boundingsphere for each sphere

        public Boundary()
        {
        }

        public void makeBox(Vector3 min, Vector3 max)
        {// function to make a boundingbox for the billboard

            billboardBox = new BoundingBox(min, max);

        }

        public void maketreeSphere(int num, Model model, Vector3 trans,float scale )
        {// function to make a array of bounding spheres
            
            foreach (ModelMesh mesh in model.Meshes)
            {

                foreach (BasicEffect effect in mesh.Effects)
                {
                    treesphere[num] = mesh.BoundingSphere.Transform(effect.World);
                    treesphere[num].Radius = 2.5f;
                    TreeBoxes[num] = BoundingBox.CreateFromSphere(treesphere[num]);
                    TreeBoxes[num].Min.Y = 0;
                    TreeBoxes[num].Max.Y = 40;
                }
                
            }
        }


        public void makeSphere(int num, Vector3 position, float radius)
        {// function to make a array of bounding spheres

            boundSphere[num] = new BoundingSphere(position, radius);

        }

        public Boolean checkTreeCollision()
        {// function to check collision between spheres and boxes

            foreach (BoundingSphere sphere in boundSphere)
            { // checks each sphere in the boundingsphere array

                foreach (BoundingBox tree in TreeBoxes)
                {// checks each tree in the treebox array 

                    if (sphere.Intersects(tree))
                    {// checks if the sphere element touches treebox element

                        return true;

                    }

                    if (billboardBox.Intersects(tree))
                        return true;
                }
            }
           

            return false;  // if no collision returns false
        }


        public Boolean BillboardCollision()
        {
            foreach (BoundingSphere sphere in boundSphere)
            { // checks each sphere in the boundingsphere array
                if (sphere.Intersects(billboardBox))
                {
                    return true;
                }
            }

            foreach (BoundingBox tree in TreeBoxes)
            {// checks each tree in the treebox array 

                if (billboardBox.Intersects(tree))
                    return true;
            }
            return false;

        }

        public BoundingBox box
        {
            get { return billboardBox; }
            set { billboardBox = value; }
        }

        public BoundingBox[] TreeBox
        {
            get { return TreeBoxes; }
            set { TreeBoxes = value; }
        }

        public BoundingSphere[] BoundSphere
        {
            get { return boundSphere; }
            set { boundSphere = value; }
        }

        public BoundingSphere[] TreeSphere
        {
            get { return treesphere; }
            set { treesphere = value; }
        }

    }

}

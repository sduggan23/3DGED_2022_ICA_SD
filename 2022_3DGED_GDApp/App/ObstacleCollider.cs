using GD.Engine.Events;
using GD.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD.App
{
    public class ObstacleCollider : Collider
    {
        private bool collided = false;

        public ObstacleCollider(GameObject gameObject,
            bool isHandlingCollision = false, bool isTrigger = false)
            : base(gameObject, isHandlingCollision, isTrigger)
        {
        }

        protected override void HandleResponse(GameObject parentGameObject)
        {
            CollidableFirstPersonController controller = parentGameObject.GetComponent<CollidableFirstPersonController>();

            if (controller == null)
            {
                if (parentGameObject.Name == AppData.THIRD_PERSON_CAMERA_NAME)
                    System.Diagnostics.Debug.WriteLine("CollidableFirstPersonController not found.");
                return;
            }

            // If collided with player, play audio
            if (!collided)
            {
                object[] parameters = { "Explode" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Player, EventActionType.OnPlay2D, parameters));

                //collided = true;
            }
        }
    }
}
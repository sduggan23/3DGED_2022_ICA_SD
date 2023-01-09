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
            Player controller = parentGameObject.GetComponent<Player>();

            if (controller == null)
            {
                if (parentGameObject.Name == AppData.THIRD_PERSON_CAMERA_NAME)
                    System.Diagnostics.Debug.WriteLine("Player not found.");
                return;
            }

            // If collided with player, play audio
            if (!collided)
            {
                object[] parameters = { "Explode" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Player, EventActionType.OnPlay2D, parameters));


                object[] parametersBG = { "BGMusic" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Player, EventActionType.OnPause, parametersBG));

                object[] parametersP = { "Engine" };
                EventDispatcher.Raise(new EventData(EventCategoryType.Player, EventActionType.OnPause, parametersP));
                EventDispatcher.Raise(new EventData(EventCategoryType.Menu, EventActionType.OnPause));
                EventDispatcher.Raise(new EventData(EventCategoryType.Menu, EventActionType.OnEnterLevelFailedUI));

                //collided = true;
            }
        }
    }
}
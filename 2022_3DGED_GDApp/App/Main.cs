#region Pre-compiler directives

#define DEMO
#define SHOW_DEBUG_INFO

#endregion

using App.Managers;
using GD.Core;
using GD.Engine;
using GD.Engine.Events;
using GD.Engine.Globals;
using GD.Engine.Inputs;
using GD.Engine.Managers;
using GD.Engine.Parameters;
using GD.Engine.Utilities;
using JigLibX.Collision;
using JigLibX.Geometry;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text;
using Application = GD.Engine.Globals.Application;
using Cue = GD.Engine.Managers.Cue;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace GD.App
{
    public class Main : Game
    {
        #region Fields

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private BasicEffect unlitEffect;
        private BasicEffect litEffect;

        private CameraManager cameraManager;
        private SceneManager<Scene> sceneManager;
        private SoundManager soundManager;
        private PhysicsManager physicsManager;
        private RenderManager renderManager;
        private EventDispatcher eventDispatcher;
        private GameObject playerGameObject;
        private PickingManager pickingManager;
        private MyStateManager stateManager;
        private SceneManager<Scene2D> uiManager;
        private SceneManager<Scene2D> menuManager;

#if DEMO

        private event EventHandler OnChanged;

#endif

        #endregion Fields

        #region Constructors

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        #endregion Constructors

        #region Actions - Initialize

#if DEMO

        private void DemoCode()
        {
            //shows how we can create an event, register for it, and raise it in Main::Update() on Keys.E press
            DemoEvent();

            ////shows us how to listen to a specific event
            //DemoStateManagerEvent();

        }

        private void HandleEvent(EventData eventData)
        {
            switch (eventData.EventActionType)
            {

                case EventActionType.OnPlay2D:
                    System.Diagnostics.Debug.WriteLine(eventData.Parameters[0] as string);
                    break;

                case EventActionType.OnWin:
                    System.Diagnostics.Debug.WriteLine(eventData.Parameters[0] as string);
                    break;

                case EventActionType.OnLose:
                    System.Diagnostics.Debug.WriteLine(eventData.Parameters[2] as string);
                    break;

                default:
                    break;
            }
        }

        private void DemoEvent()
        {
            OnChanged += HandleOnChanged;
        }

        private void HandleOnChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"{e} was sent by {sender}");
        }

        private void InitializeEvents()
        {
            EventDispatcher.Raise(new EventData(EventCategoryType.Menu, EventActionType.OnPause));
            EventDispatcher.Subscribe(EventCategoryType.Menu, HandleEnterLevelCompleteUI);
            EventDispatcher.Subscribe(EventCategoryType.Menu, HandleEnterLevelFailedUI);
            EventDispatcher.Subscribe(EventCategoryType.Menu, HandleExitLevel);
        }

#endif

        protected override void Initialize()
        {
            //moved spritebatch initialization here because we need it in InitializeDebug() below
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //core engine - common across any game
            InitializeEngine(AppData.APP_RESOLUTION, true, true);

            //game specific content
            InitializeLevel("Runner", AppData.SKYBOX_WORLD_SCALE);

            base.Initialize();
        }

        #endregion Actions - Initialize

        #region Actions - Level Specific

        protected override void LoadContent()
        {
            //moved spritebatch initialization to Main::Initialize() because we need it in InitializeDebug()
            //_spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        private void InitializeLevel(string title, float worldScale)
        {
            //set game title
            SetTitle(title);
            InitializeEvents();

            //add scene manager and starting scenes
            InitializeScenes();

            InitializeMainMenu();

            //add collidable drawn stuff
            InitializeCollidableContent(worldScale);

            //add non-collidable drawn stuff
            InitializeNonCollidableContent(worldScale);

            InitializeUI();
            LoadSounds();

            //add the player
            InitializePlayer();
        }

        private void SetTitle(string title)
        {
            Window.Title = title.Trim();
        }

        private void HandleEnterLevelFailedUI(EventData eventData)
        {
            if (eventData.EventActionType == EventActionType.OnEnterLevelFailedUI)
            {
                InitializeLevelFailedUI();
            }
        }

        private void HandleEnterLevelCompleteUI(EventData eventData)
        {
            if (eventData.EventActionType == EventActionType.OnEnterLevelCompleteUI)
            {
                InitializeLevelCompleteUI();
            }
        }

        private void HandleExitLevel(EventData eventData)
        {
            if (eventData.EventActionType == EventActionType.OnExit)
            {
                Exit();
            }
        }

        private void InitializeScenes()
        {
            //initialize a scene
            var scene = new Scene("level01");

            //add scene to the scene manager
            sceneManager.Add(scene.ID, scene);

            //don't forget to set active scene
            sceneManager.SetActiveScene("level01");
        }

        private void InitializeMainMenu()
        {
            GameObject menuGameObject = null;
            Material2D material = null;
            Renderer2D renderer2D = null;
            Texture2D btnTexture = Content.Load<Texture2D>("Assets/Textures/Menu/Controls/genericbtn");
            Texture2D backGroundtexture = Content.Load<Texture2D>("Assets/Textures/Menu/Backgrounds/main_menu_bg_1280x720");
            SpriteFont spriteFont = Content.Load<SpriteFont>("Assets/Fonts/Audiowide-Regular");
            Vector2 btnScale = Vector2.One;

            #region Create new menu scene

            //add new main menu scene
            var mainMenuScene = new Scene2D("main menu");

            #endregion

            #region Add Background Texture

            menuGameObject = new GameObject("background");
            var scaleToWindow = _graphics.GetScaleFactorForResolution(backGroundtexture, Vector2.Zero);
            //set transform
            menuGameObject.Transform = new Transform(
                new Vector3(scaleToWindow, 1), //s
                new Vector3(0, 0, 0), //r
                new Vector3(0, 0, 0)); //t

            #region texture

            //material and renderer
            material = new TextureMaterial2D(backGroundtexture, Color.White, 1);
            menuGameObject.AddComponent(new Renderer2D(material));

            #endregion

            //add to scene2D
            mainMenuScene.Add(menuGameObject);

            #endregion

            #region Add Play button and text

            menuGameObject = new GameObject("play");
            menuGameObject.Transform = new Transform(
            new Vector3(btnScale, 1), //s
            new Vector3(0, 0, 0), //r
            new Vector3(Application.Screen.ScreenCentre - btnScale * btnTexture.GetCenter() + new Vector2(0, -45), 0)); //t

            #region texture

            //material and renderer
            material = new TextureMaterial2D(btnTexture, Color.Orange, 0.9f);
            //add renderer to draw the texture
            renderer2D = new Renderer2D(material);
            //add renderer as a component
            menuGameObject.AddComponent(renderer2D);

            #endregion

            #region collider

            //add bounding box for mouse collisions using the renderer for the texture (which will automatically correctly size the bounding box for mouse interactions)
            var buttonCollider2D = new ButtonCollider2D(menuGameObject, renderer2D);
            //add any events on MouseButton (e.g. Left, Right, Hover)
            buttonCollider2D.AddEvent(MouseButton.Left, new EventData(EventCategoryType.Menu, EventActionType.OnPlay));
            menuGameObject.AddComponent(buttonCollider2D);

            #endregion

            #region text

            //material and renderer
            material = new TextMaterial2D(spriteFont, "Start", new Vector2(100, 25), Color.White, 0.8f);
            //add renderer to draw the text
            renderer2D = new Renderer2D(material);
            menuGameObject.AddComponent(renderer2D);

            #endregion

            #region color change button

            //menuGameObject.AddComponent(new UIColorFlipOnTimeBehaviour(Color.Green, Color.White, 500));

            #endregion

            //add to scene2D
            mainMenuScene.Add(menuGameObject);

            #endregion

            #region Add Exit button and text

            menuGameObject = new GameObject("exit");

            menuGameObject.Transform = new Transform(
                new Vector3(btnScale, 1), //s
                new Vector3(0, 0, 0), //r
                new Vector3(Application.Screen.ScreenCentre - btnScale * btnTexture.GetCenter() + new Vector2(0, 90), 0)); //t

            #region texture

            //material and renderer
            material = new TextureMaterial2D(btnTexture, Color.Orange, 0.9f);
            //add renderer to draw the texture
            renderer2D = new Renderer2D(material);
            //add renderer as a component
            menuGameObject.AddComponent(renderer2D);

            #endregion

            #region collider

            //add bounding box for mouse collisions using the renderer for the texture (which will automatically correctly size the bounding box for mouse interactions)
            buttonCollider2D = new ButtonCollider2D(menuGameObject, renderer2D);
            //add any events on MouseButton (e.g. Left, Right, Hover)
            buttonCollider2D.AddEvent(MouseButton.Left, new EventData(EventCategoryType.Menu, EventActionType.OnExit));
            menuGameObject.AddComponent(buttonCollider2D);

            #endregion

            #region text

            //button material and renderer
            material = new TextMaterial2D(spriteFont, "Exit", new Vector2(100, 25), Color.White, 0.8f);
            //add renderer to draw the text
            renderer2D = new Renderer2D(material);
            menuGameObject.AddComponent(renderer2D);

            #endregion

            //add to scene2D
            mainMenuScene.Add(menuGameObject);

            #endregion

            #region Add Scene to Manager and Set Active

            //add scene2D to menu manager
            menuManager.Add(mainMenuScene.ID, mainMenuScene);

            //what menu do i see first?
            menuManager.SetActiveScene(mainMenuScene.ID);

            #endregion
        }

        private void InitializeCameras()
        {
            //camera
            GameObject cameraGameObject = null;
            InitializeCurves();

            #region Third Person

            cameraGameObject = new GameObject(AppData.THIRD_PERSON_CAMERA_NAME);
            cameraGameObject.Transform = new Transform(null, null, null);
            cameraGameObject.AddComponent(new Camera(
                AppData.FIRST_PERSON_HALF_FOV, //MathHelper.PiOver2 / 2,
                (float)_graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight,
                AppData.FIRST_PERSON_CAMERA_NCP, //0.1f,
                AppData.FIRST_PERSON_CAMERA_FCP,
                new Viewport(0, 0, _graphics.PreferredBackBufferWidth,
                _graphics.PreferredBackBufferHeight))); // 3000

            cameraGameObject.AddComponent(new ThirdPersonController());

            cameraManager.Add(cameraGameObject.Name, cameraGameObject);

            cameraGameObject.AddComponent(new AudioListenerBehaviour());

            //cameraManager.SetActiveCamera(AppData.THIRD_PERSON_CAMERA_NAME);

            #endregion

            #region First Person

            //camera 1
            cameraGameObject = new GameObject(AppData.FIRST_PERSON_CAMERA_NAME);
            cameraGameObject.Transform = new Transform(null, null,
                AppData.FIRST_PERSON_DEFAULT_CAMERA_POSITION);

            #region Camera - View & Projection

            cameraGameObject.AddComponent(
             new Camera(
             AppData.FIRST_PERSON_HALF_FOV, //MathHelper.PiOver2 / 2,
             (float)_graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight,
             AppData.FIRST_PERSON_CAMERA_NCP, //0.1f,
             AppData.FIRST_PERSON_CAMERA_FCP,
             new Viewport(0, 0, _graphics.PreferredBackBufferWidth,
             _graphics.PreferredBackBufferHeight))); // 3000

            #endregion

            #region Collision - Add capsule

            //adding a collidable surface that enables acceleration, jumping
            var characterCollider = new CharacterCollider(cameraGameObject, true);

            cameraGameObject.AddComponent(characterCollider);
            characterCollider.AddPrimitive(new Capsule(
                cameraGameObject.Transform.Translation,
                Matrix.CreateRotationX(MathHelper.PiOver2),
                1, 3.6f),
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            characterCollider.Enable(cameraGameObject, false, 1);

            #endregion

            #region Collision - Add Controller for movement (now with collision)

            cameraGameObject.AddComponent(new CollidableFirstPersonController(cameraGameObject,
                characterCollider,
                AppData.FIRST_PERSON_MOVE_SPEED, AppData.FIRST_PERSON_STRAFE_SPEED,
                AppData.PLAYER_ROTATE_SPEED_VECTOR2, AppData.FIRST_PERSON_CAMERA_SMOOTH_FACTOR, true,
                AppData.PLAYER_COLLIDABLE_JUMP_HEIGHT));

            #endregion

            #region 3D Sound

            //added ability for camera to listen to 3D sounds
            cameraGameObject.AddComponent(new AudioListenerBehaviour());

            #endregion

            cameraManager.Add(cameraGameObject.Name, cameraGameObject);

            #endregion First Person

        }

        private void InitializeCurves()
        {
            //camera
            GameObject cameraGameObject = null;

            Curve3D curve3D = new Curve3D(CurveLoopType.Constant);
            curve3D.Add(new Vector3(-12, 1, -100), 0);
            curve3D.Add(new Vector3(12, 7.5f, -75), 5000);
            curve3D.Add(new Vector3(-12, 7, -50), 10000);
            curve3D.Add(new Vector3(0, 4.25F, 0), 15000);

            cameraGameObject = new GameObject(AppData.CURVE_CAMERA_NAME);
            cameraGameObject.Transform =
                new Transform(null, new Vector3(0, 180, 0), null);
            cameraGameObject.AddComponent(new Camera(
                MathHelper.PiOver2 / 2,
                (float)_graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight,
                0.1f, 3500,
                  new Viewport(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight)));

            //define what action the curve will apply to the target game object
            var curveAction = (Curve3D curve, GameObject target, GameTime gameTime) =>
            {
                target.Transform.SetTranslation(curve.Evaluate(gameTime.TotalGameTime.TotalMilliseconds, 4));
            };

            cameraGameObject.AddComponent(new CurveBehaviour(curve3D, curveAction));
            cameraManager.Add(cameraGameObject.Name, cameraGameObject);
            cameraManager.SetActiveCamera(AppData.CURVE_CAMERA_NAME);
        }

        private void InitializeCollidableContent(float worldScale)
        {
            InitializeCollidableGround(worldScale);
            InitializeCollidableLevel01();
            InitializeLevel01Obstacles();
            InitializeFinishLineTrigger();
        }

        private void InitializeNonCollidableContent(float worldScale)
        {
            InitializeSkyBox(worldScale);
            InitializeTutorialUI();
        }

        private void InitializeUI()
        {
            #region UI Variables
            GameObject uiGameObject = null;
            Material2D material = null;
            Texture2D texture = Content.Load<Texture2D>("Assets/Textures/Menu/Controls/progress_white");

            var mainHUD = new Scene2D("game HUD");

            #endregion UI Variables

            #region Add UI Element

            uiGameObject = new GameObject("progress bar - health - 1");
            uiGameObject.Transform = new Transform(
                new Vector3(1, 1, 0), //s
                new Vector3(0, 0, 0), //r
                new Vector3(_graphics.PreferredBackBufferWidth - texture.Width - 20,
                20, 0)); //t

            //add to scene2D
            mainHUD.Add(uiGameObject);

            #endregion

            #region Add Scene to Manager and Set Active

            //add scene2D to manager
            uiManager.Add(mainHUD.ID, mainHUD);

            //what ui do i see first?
            uiManager.SetActiveScene(mainHUD.ID);

            #endregion
        }

        private void InitializeTutorialUI()
        {
            #region Tutorial 0 UI

            var levelUI = new GameObject("Tutorial 0 UI",
                ObjectType.Static, RenderType.Opaque);
            levelUI.Transform = new Transform(new Vector3(29, 10, 1), null,
                new Vector3(0, 5, 0));  //World
            var texture = Content.Load<Texture2D>("Assets/Textures/Menu/Backgrounds/tutorial0");
            levelUI.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            sceneManager.ActiveScene.Add(levelUI);

            #endregion Tutorial 0 UI

            #region Tutorial 1 UI

            levelUI = new GameObject("Tutorial 1 UI",
                ObjectType.Static, RenderType.Opaque);
            levelUI.Transform = new Transform(new Vector3(29, 10, 1), null,
                new Vector3(0, 10, 15));  //World
            texture = Content.Load<Texture2D>("Assets/Textures/Menu/Backgrounds/tutorial1");
            levelUI.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            sceneManager.ActiveScene.Add(levelUI);

            #endregion Tutorial 1 UI

            #region Tutorial 2 UI

            levelUI = new GameObject("Tutorial 2 UI",
                ObjectType.Static, RenderType.Opaque);
            levelUI.Transform = new Transform(new Vector3(29, 10, 1), null,
                new Vector3(0, 10, 225));  //World
            texture = Content.Load<Texture2D>("Assets/Textures/Menu/Backgrounds/tutorial2");
            levelUI.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            sceneManager.ActiveScene.Add(levelUI);

            #endregion Tutorial 2 UI

            #region Tutorial 3 UI

            levelUI = new GameObject("Tutorial 2 UI",
                ObjectType.Static, RenderType.Opaque);
            levelUI.Transform = new Transform(new Vector3(29, 10, 1), null,
                new Vector3(0, 10, 1000));  //World
            texture = Content.Load<Texture2D>("Assets/Textures/Menu/Backgrounds/tutorial3");
            levelUI.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            sceneManager.ActiveScene.Add(levelUI);

            #endregion Tutorial 3 UI
        }

        private void InitializeLevelCompleteUI()
        {
            GameObject menuGameObject = null;
            Material2D material = null;
            Renderer2D renderer2D = null;
            Texture2D btnTexture = Content.Load<Texture2D>("Assets/Textures/Menu/Controls/genericbtn");
            Texture2D backGroundtexture = Content.Load<Texture2D>("Assets/Textures/Menu/Backgrounds/levelcomplete");
            SpriteFont spriteFont = Content.Load<SpriteFont>("Assets/Fonts/Audiowide-Regular");
            Vector2 btnScale = Vector2.One;

            #region Create new menu scene

            //add new main menu scene
            var levelFailedUI = new Scene2D("level complete UI");

            #endregion

            #region Add Background Texture

            menuGameObject = new GameObject("background");
            var scaleToWindow = _graphics.GetScaleFactorForResolution(backGroundtexture, Vector2.Zero);
            //set transform
            menuGameObject.Transform = new Transform(
                new Vector3(scaleToWindow, 1), //s
                new Vector3(0, 0, 0), //r
                new Vector3(0, 0, 0)); //t

            #region texture

            //material and renderer
            material = new TextureMaterial2D(backGroundtexture, Color.White, 1);
            menuGameObject.AddComponent(new Renderer2D(material));

            #endregion

            //add to scene2D
            levelFailedUI.Add(menuGameObject);

            #endregion

            #region Add Play button and text

            menuGameObject = new GameObject("exit");
            menuGameObject.Transform = new Transform(
            new Vector3(btnScale, 1), //s
            new Vector3(0, 0, 0), //r
            new Vector3(Application.Screen.ScreenCentre - btnScale * btnTexture.GetCenter() + new Vector2(-0, 250), 0)); //t

            #region texture

            //material and renderer
            material = new TextureMaterial2D(btnTexture, Color.White, 0.9f);
            //add renderer to draw the texture
            renderer2D = new Renderer2D(material);
            //add renderer as a component
            menuGameObject.AddComponent(renderer2D);

            #endregion

            #region collider

            //add bounding box for mouse collisions using the renderer for the texture (which will automatically correctly size the bounding box for mouse interactions)
            var buttonCollider2D = new ButtonCollider2D(menuGameObject, renderer2D);
            //add any events on MouseButton (e.g. Left, Right, Hover)
            buttonCollider2D.AddEvent(MouseButton.Left, new EventData(EventCategoryType.Menu, EventActionType.OnExit));
            menuGameObject.AddComponent(buttonCollider2D);

            #endregion

            #region text

            //material and renderer
            material = new TextMaterial2D(spriteFont, "Quit", new Vector2(100, 25), Color.White, 0.8f);
            //add renderer to draw the text
            renderer2D = new Renderer2D(material);
            menuGameObject.AddComponent(renderer2D);

            #endregion

            //add to scene2D
            levelFailedUI.Add(menuGameObject);

            #endregion

            #region Add Scene to Manager and Set Active

            //add scene2D to menu manager
            menuManager.Add(levelFailedUI.ID, levelFailedUI);

            //what menu do i see first?
            menuManager.SetActiveScene(levelFailedUI.ID);

            #endregion
        }

        private void InitializeLevelFailedUI()
        {
            GameObject menuGameObject = null;
            Material2D material = null;
            Renderer2D renderer2D = null;
            Texture2D btnTexture = Content.Load<Texture2D>("Assets/Textures/Menu/Controls/genericbtn");
            Texture2D backGroundtexture = Content.Load<Texture2D>("Assets/Textures/Menu/Backgrounds/levelfailed");
            SpriteFont spriteFont = Content.Load<SpriteFont>("Assets/Fonts/Audiowide-Regular");
            Vector2 btnScale = Vector2.One;

            #region Create new menu scene

            //add new main menu scene
            var levelFailedUI = new Scene2D("level failed UI");

            #endregion

            #region Add Background Texture

            menuGameObject = new GameObject("background");
            var scaleToWindow = _graphics.GetScaleFactorForResolution(backGroundtexture, Vector2.Zero);
            //set transform
            menuGameObject.Transform = new Transform(
                new Vector3(scaleToWindow, 1), //s
                new Vector3(0, 0, 0), //r
                new Vector3(0, 0, 0)); //t

            #region texture

            //material and renderer
            material = new TextureMaterial2D(backGroundtexture, Color.White, 1);
            menuGameObject.AddComponent(new Renderer2D(material));

            #endregion

            //add to scene2D
            levelFailedUI.Add(menuGameObject);

            #endregion

            #region Add Play button and text

            menuGameObject = new GameObject("exit");
            menuGameObject.Transform = new Transform(
            new Vector3(btnScale, 1), //s
            new Vector3(0, 0, 0), //r
            new Vector3(Application.Screen.ScreenCentre - btnScale * btnTexture.GetCenter() + new Vector2(-0, 250), 0)); //t

            #region texture

            //material and renderer
            material = new TextureMaterial2D(btnTexture, Color.White, 0.9f);
            //add renderer to draw the texture
            renderer2D = new Renderer2D(material);
            //add renderer as a component
            menuGameObject.AddComponent(renderer2D);

            #endregion

            #region collider

            //add bounding box for mouse collisions using the renderer for the texture (which will automatically correctly size the bounding box for mouse interactions)
            var buttonCollider2D = new ButtonCollider2D(menuGameObject, renderer2D);
            //add any events on MouseButton (e.g. Left, Right, Hover)
            buttonCollider2D.AddEvent(MouseButton.Left, new EventData(EventCategoryType.Menu, EventActionType.OnExit));
            menuGameObject.AddComponent(buttonCollider2D);

            #endregion

            #region text

            //material and renderer
            material = new TextMaterial2D(spriteFont, "Quit", new Vector2(100, 25), Color.White, 0.8f);
            //add renderer to draw the text
            renderer2D = new Renderer2D(material);
            menuGameObject.AddComponent(renderer2D);

            #endregion

            //add to scene2D
            levelFailedUI.Add(menuGameObject);

            #endregion

            #region Add Scene to Manager and Set Active

            //add scene2D to menu manager
            menuManager.Add(levelFailedUI.ID, levelFailedUI);

            //what menu do i see first?
            menuManager.SetActiveScene(levelFailedUI.ID);

            #endregion
        }

        private void InitializeDistanceMeter()
        {
            //intialize the utility component
            var perfUtility = new PerfUtility(this, _spriteBatch,
                new Vector2(20, 20),
                new Vector2(0, 22));

            //set the font to be used
            var spriteFont = Content.Load<SpriteFont>("Assets/Fonts/Audiowide-Regular");

            //add components to the info list to add UI information
            float contentScale = 2f;
            var infoFunction = (Transform transform) =>
            {
                return transform.Translation.GetNewRounded(0).Z.ToString();

            };

            perfUtility.infoList.Add(new TransformInfo(_spriteBatch, spriteFont, "Distance:", Color.White, contentScale * Vector2.One,
                ref Application.CameraManager.ActiveCamera.transform, infoFunction));


            //add to the component list otherwise it wont have its Update or Draw called!
            // perfUtility.StatusType = StatusType.Drawn | StatusType.Updated;
            perfUtility.DrawOrder = 3;
            Components.Add(perfUtility);
        }
        private void LoadSounds()
        {
            #region Background Audio

            object[] parametersBG = { "BGMusic" };
            EventDispatcher.Raise(
                new EventData(EventCategoryType.Player,
                EventActionType.OnPlay,
                parametersBG));

            var sound = Content.Load<SoundEffect>("Assets/Audio/Non-Digetic/cigaro30__synthwave-beat");
            //Add the new sound for background
            soundManager.Add(new Cue(
                "BGMusic",
                 sound,
                 SoundCategoryType.BackgroundMusic,
                 new Vector3(0.1f, 0, 0),
                 true));

            #endregion Background Audio

            #region SoundEffects

            object[] parameters = { "Engine" };
            EventDispatcher.Raise(
                new EventData(EventCategoryType.Player,
                EventActionType.OnPlay2D,
                parameters));

            var soundEffect = Content.Load<SoundEffect>("Assets/Audio/Diegetic/Engine Hum");
            //Add the new sound for background
            soundManager.Add(new Cue(
                "Engine",
                 soundEffect,
                 SoundCategoryType.Alarm,
                 new Vector3(0.1f, 0, 0),
                 true));


            soundEffect = Content.Load<SoundEffect>("Assets/Audio/Diegetic/superphat__scifiheavyblastershot");
            //Add the new sound for background
            soundManager.Add(new Cue(
                "Explode",
                 soundEffect,
                 SoundCategoryType.Alarm,
                 new Vector3(0.1f, 0, 0),
                 false));

            soundEffect = Content.Load<SoundEffect>("Assets/Audio/Diegetic/copyc4t__levelup");
            //Add the new sound for background
            soundManager.Add(new Cue(
                "LevelComplete",
                 soundEffect,
                 SoundCategoryType.Alarm,
                 new Vector3(0.1f, 0, 0),
                 false));

            #endregion SoundEffects

        }

        private void InitializeCollidableGround(float worldScale)
        {
            var gdBasicEffect = new GDBasicEffect(unlitEffect);
            var quadMesh = new QuadMesh(_graphics.GraphicsDevice);

            //ground
            var ground = new GameObject("ground");
            ground.Transform = new Transform(new Vector3(worldScale, worldScale, 1),
                new Vector3(-90, 0, 0), new Vector3(0, 0, 0));
            var texture = Content.Load<Texture2D>("Assets/Textures/Skybox/ground");
            ground.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));

            //add Collision Surface(s)
            var collider = new Collider(ground);
            collider.AddPrimitive(new Box(
                    ground.Transform.Translation,
                    ground.Transform.Rotation,
                    ground.Transform.Scale),
                    new MaterialProperties(0.8f, 0.8f, 0.7f));
            collider.Enable(ground, true, 1);
            ground.AddComponent(collider);

            sceneManager.ActiveScene.Add(ground);
        }

        private void InitializeCollidableLevel01()
        {
            #region wallR
            var wall = new GameObject("Collidable wall right", ObjectType.Static, RenderType.Opaque);
            wall.GameObjectType = GameObjectType.Architecture;
            wall.Transform = new Transform
                (new Vector3(1, 10f, 1100),
                new Vector3(0, 0, 30),
                new Vector3(15f, 5f, 450));
            var texture = Content.Load<Texture2D>("Assets/Textures/Level/gridpurple200%");

            wall.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            var wallCollider = new Collider(wall, true);
            wallCollider.AddPrimitive(new Box(
                wall.Transform.Translation,
                wall.Transform.Rotation,
                wall.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            wallCollider.Enable(wall, true, 10);
            wall.AddComponent(wallCollider);

            sceneManager.ActiveScene.Add(wall);

            wall = new GameObject("Collidable small wall right", ObjectType.Static, RenderType.Opaque);
            wall.GameObjectType = GameObjectType.Architecture;
            wall.Transform = new Transform
                (new Vector3(1, 10f, 1100),
                null,
                new Vector3(15f, 5f, 450));

            wall.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            wallCollider = new Collider(wall, true);
            wallCollider.AddPrimitive(new Box(
                wall.Transform.Translation,
                wall.Transform.Rotation,
                wall.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            wallCollider.Enable(wall, true, 10);
            wall.AddComponent(wallCollider);

            sceneManager.ActiveScene.Add(wall);

            #endregion wallR

            #region wallL

            wall = new GameObject("Collidable wall left", ObjectType.Static, RenderType.Opaque);
            wall.GameObjectType = GameObjectType.Architecture;
            wall.Transform = new Transform
                (new Vector3(1, 10f, 1100),
                new Vector3(0, 0, -30),
                new Vector3(-15f, 5f, 460));

            wall.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            wallCollider = new Collider(wall, true);
            wallCollider.AddPrimitive(new Box(
                wall.Transform.Translation,
                wall.Transform.Rotation,
                wall.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            wallCollider.Enable(wall, true, 10);
            wall.AddComponent(wallCollider);

            sceneManager.ActiveScene.Add(wall);

            wall = new GameObject("Collidable small wall left", ObjectType.Static, RenderType.Opaque);
            wall.GameObjectType = GameObjectType.Architecture;
            wall.Transform = new Transform
                (new Vector3(1, 10f, 1100),
                null,
                new Vector3(-15f, 5f, 460));

            wall.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            wallCollider = new Collider(wall, true);
            wallCollider.AddPrimitive(new Box(
                wall.Transform.Translation,
                wall.Transform.Rotation,
                wall.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            wallCollider.Enable(wall, true, 10);
            wall.AddComponent(wallCollider);

            sceneManager.ActiveScene.Add(wall);

            #endregion wallL

            #region ceiling

            var ceiling = new GameObject("Collidable ceiling", ObjectType.Static, RenderType.Opaque);
            ceiling.GameObjectType = GameObjectType.Architecture;
            ceiling.Transform = new Transform
                (new Vector3(1, 30f, 1100),
                new Vector3(0, 0, 90),
                new Vector3(0, 9.5f, 450));

            ceiling.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            var ceilingCollider = new Collider(ceiling, true);
            ceilingCollider.AddPrimitive(new Box(
                ceiling.Transform.Translation,
                ceiling.Transform.Rotation,
                ceiling.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            ceilingCollider.Enable(ceiling, true, 10);
            ceiling.AddComponent(ceilingCollider);

            sceneManager.ActiveScene.Add(ceiling);

            #endregion ceiling

            #region floor

            var floor = new GameObject("Collidable floor", ObjectType.Static, RenderType.Opaque);
            floor.GameObjectType = GameObjectType.Architecture;
            floor.Transform = new Transform
                (new Vector3(1, 40f, 1100),
                new Vector3(0, 0, 90),
                new Vector3(0, -.45f, 460));

            floor.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            var floorCollider = new Collider(floor, true);
            floorCollider.AddPrimitive(new Box(
                floor.Transform.Translation,
                floor.Transform.Rotation,
                floor.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            floorCollider.Enable(floor, true, 10);
            ceiling.AddComponent(floorCollider);

            sceneManager.ActiveScene.Add(floor);

            #endregion floor

            InitializeCollidableLevel02();
        }

        private void InitializeCollidableLevel02()
        {
            #region wallR
            var wall = new GameObject("Collidable wall right", ObjectType.Static, RenderType.Opaque);
            wall.GameObjectType = GameObjectType.Architecture;
            wall.Transform = new Transform
                (new Vector3(1, 10f, 1100),
                new Vector3(0, 0, 30),
                new Vector3(15f, 5f, 1550));
            var texture = Content.Load<Texture2D>("Assets/Textures/Level/gridblue200%");

            wall.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            var wallCollider = new Collider(wall, true);
            wallCollider.AddPrimitive(new Box(
                wall.Transform.Translation,
                wall.Transform.Rotation,
                wall.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            wallCollider.Enable(wall, true, 10);
            wall.AddComponent(wallCollider);

            sceneManager.ActiveScene.Add(wall);

            wall = new GameObject("Collidable small wall right", ObjectType.Static, RenderType.Opaque);
            wall.GameObjectType = GameObjectType.Architecture;
            wall.Transform = new Transform
                (new Vector3(1, 10f, 1100),
                null,
                new Vector3(15f, 5f, 1550));

            wall.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            wallCollider = new Collider(wall, true);
            wallCollider.AddPrimitive(new Box(
                wall.Transform.Translation,
                wall.Transform.Rotation,
                wall.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            wallCollider.Enable(wall, true, 10);
            wall.AddComponent(wallCollider);

            sceneManager.ActiveScene.Add(wall);

            #endregion wallR

            #region wallL

            wall = new GameObject("Collidable wall left", ObjectType.Static, RenderType.Opaque);
            wall.GameObjectType = GameObjectType.Architecture;
            wall.Transform = new Transform
                (new Vector3(1, 10f, 1100),
                new Vector3(0, 0, -30),
                new Vector3(-15f, 5f, 1560));

            wall.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            wallCollider = new Collider(wall, true);
            wallCollider.AddPrimitive(new Box(
                wall.Transform.Translation,
                wall.Transform.Rotation,
                wall.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            wallCollider.Enable(wall, true, 10);
            wall.AddComponent(wallCollider);

            sceneManager.ActiveScene.Add(wall);

            wall = new GameObject("Collidable small wall left", ObjectType.Static, RenderType.Opaque);
            wall.GameObjectType = GameObjectType.Architecture;
            wall.Transform = new Transform
                (new Vector3(1, 10f, 1100),
                null,
                new Vector3(-15f, 5f, 1560));

            wall.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            wallCollider = new Collider(wall, true);
            wallCollider.AddPrimitive(new Box(
                wall.Transform.Translation,
                wall.Transform.Rotation,
                wall.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            wallCollider.Enable(wall, true, 10);
            wall.AddComponent(wallCollider);

            sceneManager.ActiveScene.Add(wall);

            #endregion wallL

            #region ceiling

            var ceiling = new GameObject("Collidable ceiling", ObjectType.Static, RenderType.Opaque);
            ceiling.GameObjectType = GameObjectType.Architecture;
            ceiling.Transform = new Transform
                (new Vector3(1, 30f, 1100),
                new Vector3(0, 0, 90),
                new Vector3(0, 9.5f, 1550));

            ceiling.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            var ceilingCollider = new Collider(ceiling, true);
            ceilingCollider.AddPrimitive(new Box(
                ceiling.Transform.Translation,
                ceiling.Transform.Rotation,
                ceiling.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            ceilingCollider.Enable(ceiling, true, 10);
            ceiling.AddComponent(ceilingCollider);

            sceneManager.ActiveScene.Add(ceiling);

            #endregion ceiling

            #region floor

            var floor = new GameObject("Collidable floor", ObjectType.Static, RenderType.Opaque);
            floor.GameObjectType = GameObjectType.Architecture;
            floor.Transform = new Transform
                (new Vector3(1, 40f, 1100),
                new Vector3(0, 0, 90),
                new Vector3(0, -.45f, 1560));

            floor.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            var floorCollider = new Collider(floor, true);
            floorCollider.AddPrimitive(new Box(
                floor.Transform.Translation,
                floor.Transform.Rotation,
                floor.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            floorCollider.Enable(floor, true, 10);
            ceiling.AddComponent(floorCollider);

            sceneManager.ActiveScene.Add(floor);

            #endregion floor

            #region wallR
            wall = new GameObject("Collidable wall right", ObjectType.Static, RenderType.Opaque);
            wall.GameObjectType = GameObjectType.Architecture;
            wall.Transform = new Transform
                (new Vector3(1, 10f, 1100),
                new Vector3(0, 0, 30),
                new Vector3(15f, 5f, 2650));

            wall.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            wallCollider = new Collider(wall, true);
            wallCollider.AddPrimitive(new Box(
                wall.Transform.Translation,
                wall.Transform.Rotation,
                wall.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            wallCollider.Enable(wall, true, 10);
            wall.AddComponent(wallCollider);

            sceneManager.ActiveScene.Add(wall);

            wall = new GameObject("Collidable small wall right", ObjectType.Static, RenderType.Opaque);
            wall.GameObjectType = GameObjectType.Architecture;
            wall.Transform = new Transform
                (new Vector3(1, 10f, 1100),
                null,
                new Vector3(15f, 5f, 2650));

            wall.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            wallCollider = new Collider(wall, true);
            wallCollider.AddPrimitive(new Box(
                wall.Transform.Translation,
                wall.Transform.Rotation,
                wall.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            wallCollider.Enable(wall, true, 10);
            wall.AddComponent(wallCollider);

            sceneManager.ActiveScene.Add(wall);

            #endregion wallR

            #region wallL

            wall = new GameObject("Collidable wall left", ObjectType.Static, RenderType.Opaque);
            wall.GameObjectType = GameObjectType.Architecture;
            wall.Transform = new Transform
                (new Vector3(1, 10f, 1100),
                new Vector3(0, 0, -30),
                new Vector3(-15f, 5f, 2660));

            wall.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            wallCollider = new Collider(wall, true);
            wallCollider.AddPrimitive(new Box(
                wall.Transform.Translation,
                wall.Transform.Rotation,
                wall.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            wallCollider.Enable(wall, true, 10);
            wall.AddComponent(wallCollider);

            sceneManager.ActiveScene.Add(wall);

            wall = new GameObject("Collidable small wall left", ObjectType.Static, RenderType.Opaque);
            wall.GameObjectType = GameObjectType.Architecture;
            wall.Transform = new Transform
                (new Vector3(1, 10f, 1100),
                null,
                new Vector3(-15f, 5f, 2660));

            wall.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            wallCollider = new Collider(wall, true);
            wallCollider.AddPrimitive(new Box(
                wall.Transform.Translation,
                wall.Transform.Rotation,
                wall.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            wallCollider.Enable(wall, true, 10);
            wall.AddComponent(wallCollider);

            sceneManager.ActiveScene.Add(wall);

            #endregion wallL

            #region ceiling

            ceiling = new GameObject("Collidable ceiling", ObjectType.Static, RenderType.Opaque);
            ceiling.GameObjectType = GameObjectType.Architecture;
            ceiling.Transform = new Transform
                (new Vector3(1, 30f, 1100),
                new Vector3(0, 0, 90),
                new Vector3(0, 9.5f, 2650));

            ceiling.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            ceilingCollider = new Collider(ceiling, true);
            ceilingCollider.AddPrimitive(new Box(
                ceiling.Transform.Translation,
                ceiling.Transform.Rotation,
                ceiling.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            ceilingCollider.Enable(ceiling, true, 10);
            ceiling.AddComponent(ceilingCollider);

            sceneManager.ActiveScene.Add(ceiling);

            #endregion ceiling

            #region floor

            floor = new GameObject("Collidable floor", ObjectType.Static, RenderType.Opaque);
            floor.GameObjectType = GameObjectType.Architecture;
            floor.Transform = new Transform
                (new Vector3(1, 40f, 1100),
                new Vector3(0, 0, 90),
                new Vector3(0, -.45f, 2660));

            floor.AddComponent(new Renderer(new GDBasicEffect(unlitEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            floorCollider = new Collider(floor, true);
            floorCollider.AddPrimitive(new Box(
                floor.Transform.Translation,
                floor.Transform.Rotation,
                floor.Transform.Scale), //make the colliders a fraction larger so that transparent boxes dont sit exactly on the ground and we end up with flicker or z-fighting
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            floorCollider.Enable(floor, true, 10);
            ceiling.AddComponent(floorCollider);

            sceneManager.ActiveScene.Add(floor);

            #endregion floor

        }

        private void InitializeLevel01Obstacles()
        {
            #region obstacle1

            Random rnd = new Random();
            int num1X = rnd.Next(-5, 5);
            int num1Z = rnd.Next(250, 500);

            var obstacleSmall = new GameObject("obstacle small 1",
                ObjectType.Static, RenderType.Opaque);
            obstacleSmall.Transform = new Transform(new Vector3(3, 3, 3), null,
                new Vector3(num1X, 1.5f, num1Z));  //World
            var texture = Content.Load<Texture2D>("Assets/Textures/Level/texture3");
            obstacleSmall.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            Collider obstacleCollider = new ObstacleCollider(obstacleSmall, true, false);
            obstacleCollider.AddPrimitive(
                new Box(
                obstacleSmall.Transform.Translation,
                obstacleSmall.Transform.Rotation,
                obstacleSmall.Transform.Scale),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            obstacleCollider.Enable(obstacleSmall, false, 10);
            obstacleSmall.AddComponent(obstacleCollider);

            obstacleSmall.AddComponent(new AudioEmitterBehaviour());

            sceneManager.ActiveScene.Add(obstacleSmall);

            #endregion obstacle1

            #region obstacle2
            int num2X = rnd.Next(-7, 0);
            int num2Z = rnd.Next(500, 750);
            obstacleSmall = new GameObject("obstacle small 2",
                ObjectType.Static, RenderType.Opaque);
            obstacleSmall.Transform = new Transform(new Vector3(3, 3, 3), null,
                new Vector3(num2X, 1.5f, num2Z));  //World
            obstacleSmall.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            //var obstacleSmall2Collider = new Collider(obstacleSmall2, true);
            //obstacleSmall2Collider.AddPrimitive(new Box(
            //    obstacleSmall2.Transform.Translation,
            //    obstacleSmall2.Transform.Rotation,
            //    obstacleSmall2.Transform.Scale),
            //    new MaterialProperties(0.8f, 0.8f, 0.7f));
            //obstacleSmall2Collider.Enable(obstacleSmall2, false, 10);
            //obstacleSmall2.AddComponent(obstacleSmall2Collider);

            //sceneManager.ActiveScene.Add(obstacleSmall2);

            obstacleCollider = new ObstacleCollider(obstacleSmall, true, false);
            obstacleCollider.AddPrimitive(
                new Box(
                obstacleSmall.Transform.Translation,
                obstacleSmall.Transform.Rotation,
                obstacleSmall.Transform.Scale),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            obstacleCollider.Enable(obstacleSmall, false, 10);
            obstacleSmall.AddComponent(obstacleCollider);

            obstacleSmall.AddComponent(new AudioEmitterBehaviour());

            sceneManager.ActiveScene.Add(obstacleSmall);

            #endregion obstacle2

            #region obstacle3
            int num3X = rnd.Next(0, 7);
            int num3Z = rnd.Next(755, 950);
            obstacleSmall = new GameObject("obstacle small 3",
                ObjectType.Static, RenderType.Opaque);
            obstacleSmall.Transform = new Transform(new Vector3(3, 3, 3), null,
                new Vector3(num3X, 1.5f, num3Z));  //World
            obstacleSmall.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            //var obstacleSmall3Collider = new Collider(obstacleSmall3, true);
            //obstacleSmall3Collider.AddPrimitive(new Box(
            //    obstacleSmall3.Transform.Translation,
            //    obstacleSmall3.Transform.Rotation,
            //    obstacleSmall3.Transform.Scale),
            //    new MaterialProperties(0.8f, 0.8f, 0.7f));
            //obstacleSmall3Collider.Enable(obstacleSmall3, false, 10);
            //obstacleSmall3.AddComponent(obstacleSmall3Collider);

            //sceneManager.ActiveScene.Add(obstacleSmall3);

            obstacleCollider = new ObstacleCollider(obstacleSmall, true, false);
            obstacleCollider.AddPrimitive(
                new Box(
                obstacleSmall.Transform.Translation,
                obstacleSmall.Transform.Rotation,
                obstacleSmall.Transform.Scale),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            obstacleCollider.Enable(obstacleSmall, false, 10);
            obstacleSmall.AddComponent(obstacleCollider);

            obstacleSmall.AddComponent(new AudioEmitterBehaviour());

            sceneManager.ActiveScene.Add(obstacleSmall);

            #endregion obstacle3

            InitializeLevel02Obstacles();

        }

        private void InitializeLevel02Obstacles()
        {
            #region jump obstacle1

            var obstacleJump = new GameObject("obstacle jump 1",
                ObjectType.Static, RenderType.Opaque);
            obstacleJump.Transform = new Transform(new Vector3(29, 2, 1), null,
                new Vector3(0, 1, 1050));  //World
            var texture = Content.Load<Texture2D>("Assets/Textures/Level/texture3");
            obstacleJump.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            Collider obstacleCollider = new ObstacleCollider(obstacleJump, true, false);
            obstacleCollider.AddPrimitive(
                new Box(
                obstacleJump.Transform.Translation,
                obstacleJump.Transform.Rotation,
                obstacleJump.Transform.Scale),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            obstacleCollider.Enable(obstacleJump, false, 10);
            obstacleJump.AddComponent(obstacleCollider);

            obstacleJump.AddComponent(new AudioEmitterBehaviour());

            sceneManager.ActiveScene.Add(obstacleJump);

            #endregion jump obstacle1

            #region jump obstacle2

            obstacleJump = new GameObject("obstacle jump 2",
                ObjectType.Static, RenderType.Opaque);
            obstacleJump.Transform = new Transform(new Vector3(29, 2, 1), null,
                new Vector3(0, 1, 1550));  //World
            obstacleJump.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            obstacleCollider = new ObstacleCollider(obstacleJump, true, false);
            obstacleCollider.AddPrimitive(
                new Box(
                obstacleJump.Transform.Translation,
                obstacleJump.Transform.Rotation,
                obstacleJump.Transform.Scale),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            obstacleCollider.Enable(obstacleJump, false, 10);
            obstacleJump.AddComponent(obstacleCollider);

            obstacleJump.AddComponent(new AudioEmitterBehaviour());

            sceneManager.ActiveScene.Add(obstacleJump);

            #endregion jump obstacle2

            #region jump obstacle3

            obstacleJump = new GameObject("obstacle jump 3",
                ObjectType.Static, RenderType.Opaque);
            obstacleJump.Transform = new Transform(new Vector3(29, 2, 1), null,
                new Vector3(0, 1, 1950));  //World
            obstacleJump.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            obstacleCollider = new ObstacleCollider(obstacleJump, true, false);
            obstacleCollider.AddPrimitive(
                new Box(
                obstacleJump.Transform.Translation,
                obstacleJump.Transform.Rotation,
                obstacleJump.Transform.Scale),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            obstacleCollider.Enable(obstacleJump, false, 10);
            obstacleJump.AddComponent(obstacleCollider);

            obstacleJump.AddComponent(new AudioEmitterBehaviour());

            sceneManager.ActiveScene.Add(obstacleJump);

            #endregion jump obstacle3

            #region jump obstacle4

            obstacleJump = new GameObject("obstacle jump 4",
                ObjectType.Static, RenderType.Opaque);
            obstacleJump.Transform = new Transform(new Vector3(29, 2, 1), null,
                new Vector3(0, 1, 2450));  //World
            obstacleJump.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            obstacleCollider = new ObstacleCollider(obstacleJump, true, false);
            obstacleCollider.AddPrimitive(
                new Box(
                obstacleJump.Transform.Translation,
                obstacleJump.Transform.Rotation,
                obstacleJump.Transform.Scale),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            obstacleCollider.Enable(obstacleJump, false, 10);
            obstacleJump.AddComponent(obstacleCollider);

            obstacleJump.AddComponent(new AudioEmitterBehaviour());

            sceneManager.ActiveScene.Add(obstacleJump);

            #endregion jump obstamcle4

            #region obstacle5

            Random rnd = new Random();
            int num1X = rnd.Next(-3, 3);
            int num1Z = rnd.Next(1200, 1500);

            var obstacleLarge = new GameObject("obstacle large 1",
                ObjectType.Static, RenderType.Opaque);
            obstacleLarge.Transform = new Transform(new Vector3(7, 7, 7), null,
                new Vector3(num1X, 0.5f, num1Z));  //World
            obstacleLarge.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1), new TetrahedronMesh(_graphics.GraphicsDevice)));

            obstacleCollider = new ObstacleCollider(obstacleLarge, true, false);
            obstacleCollider.AddPrimitive(
                new Box(
                obstacleLarge.Transform.Translation,
                obstacleLarge.Transform.Rotation,
                obstacleLarge.Transform.Scale),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            obstacleCollider.Enable(obstacleLarge, false, 10);
            obstacleLarge.AddComponent(obstacleCollider);

            obstacleLarge.AddComponent(new AudioEmitterBehaviour());

            sceneManager.ActiveScene.Add(obstacleLarge);

            #endregion obstacle5

            #region obstacle6
            int num2X = rnd.Next(-5, 0);
            int num2Z = rnd.Next(1600, 1900);

            obstacleLarge = new GameObject("obstacle large 2",
                ObjectType.Static, RenderType.Opaque);
            obstacleLarge.Transform = new Transform(new Vector3(7, 7, 7), null,
                new Vector3(num2X, 0.5f, num2Z));  //World
            obstacleLarge.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1), new OctahedronMesh(_graphics.GraphicsDevice)));

            obstacleCollider = new ObstacleCollider(obstacleLarge, true, false);
            obstacleCollider.AddPrimitive(
                new Box(
                obstacleLarge.Transform.Translation,
                obstacleLarge.Transform.Rotation,
                obstacleLarge.Transform.Scale),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            obstacleCollider.Enable(obstacleLarge, false, 10);
            obstacleLarge.AddComponent(obstacleCollider);

            obstacleLarge.AddComponent(new AudioEmitterBehaviour());

            sceneManager.ActiveScene.Add(obstacleLarge);

            #endregion obstacle6

            #region obstacle7
            int num3X = rnd.Next(0,5);
            int num3Z = rnd.Next(2000, 2400);

            obstacleLarge = new GameObject("obstacle large 3",
                ObjectType.Static, RenderType.Opaque);
            obstacleLarge.Transform = new Transform(new Vector3(7, 7, 7), null,
                new Vector3(num3X, 0.5f, num3Z));  //World
            obstacleLarge.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1), new IcosahedronMesh(_graphics.GraphicsDevice)));

            obstacleCollider = new ObstacleCollider(obstacleLarge, true, false);
            obstacleCollider.AddPrimitive(
                new Box(
                obstacleLarge.Transform.Translation,
                obstacleLarge.Transform.Rotation,
                obstacleLarge.Transform.Scale),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            obstacleCollider.Enable(obstacleLarge, false, 10);
            obstacleLarge.AddComponent(obstacleCollider);

            obstacleLarge.AddComponent(new AudioEmitterBehaviour());

            sceneManager.ActiveScene.Add(obstacleLarge);

            #endregion obstacle5

        }

        private void InitializeFinishLineTrigger()
        {
            #region finish line

            var finishLine = new GameObject("finish line",
                ObjectType.Static, RenderType.Opaque);
            finishLine.Transform = new Transform(new Vector3(29, 10, 10), null,
                new Vector3(0, 5, 2521));  //World
            var texture = Content.Load<Texture2D>("Assets/Textures/Level/checkerpattern");
            finishLine.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            Collider levelCompleteTrigger = new LevelCompleteTrigger(finishLine, true, true);
            levelCompleteTrigger.AddPrimitive(
                new Box(
                finishLine.Transform.Translation,
                finishLine.Transform.Rotation,
                finishLine.Transform.Scale),
                new MaterialProperties(0.8f, 0.8f, 0.7f)
                );

            levelCompleteTrigger.Enable(finishLine, false, 10);
            finishLine.AddComponent(levelCompleteTrigger);

            finishLine.AddComponent(new AudioEmitterBehaviour());

            sceneManager.ActiveScene.Add(finishLine);

            #endregion finish line
        }

        private void InitializePlayer()
        {
            playerGameObject = new GameObject("Player 1", ObjectType.Dynamic, RenderType.Opaque);

            playerGameObject.Transform = new Transform(null,
                null, new Vector3(0, 5, 15));
            var texture = Content.Load<Texture2D>("Assets/Textures/Level/pink");
            playerGameObject.AddComponent(new Renderer(new GDBasicEffect(litEffect),
                new Material(texture, 1), new CubeMesh(_graphics.GraphicsDevice)));

            //set this as active player
            Application.Player = playerGameObject;

            #region Collision - Add capsule

            //adding a collidable surface that enables acceleration, jumping
            var characterCollider = new CharacterCollider(playerGameObject, true);

            playerGameObject.AddComponent(characterCollider);
            characterCollider.AddPrimitive(new Box(
                playerGameObject.Transform.Translation,
                playerGameObject.Transform.Rotation,
                 new Vector3(1, 1, 1)),
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            characterCollider.Enable(playerGameObject, false, 1);

            #endregion

            #region Collision - Add Controller for movement (now with collision)

            playerGameObject.AddComponent(new Player(playerGameObject,
                characterCollider,
                AppData.THIRD_PERSON_MOVE_SPEED, AppData.THIRD_PERSON_STRAFE_SPEED,
                new Vector2(0, 0), AppData.THIRD_PERSON_CAMERA_SMOOTH_FACTOR, true,
                AppData.PLAYER_COLLIDABLE_JUMP_HEIGHT));

            #endregion

            sceneManager.ActiveScene.Add(playerGameObject);
        }

        private void InitializeSkyBox(float worldScale)
        {
            float halfWorldScale = worldScale / 2;

            GameObject quad = null;
            var gdBasicEffect = new GDBasicEffect(unlitEffect);
            var quadMesh = new QuadMesh(_graphics.GraphicsDevice);

            //skybox - back face
            quad = new GameObject("skybox back face");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1), null, new Vector3(0, 0, -halfWorldScale));
            var texture = Content.Load<Texture2D>("Assets/Textures/Skybox/Tron/tron_bk");
            quad.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));
            sceneManager.ActiveScene.Add(quad);

            //skybox - left face
            quad = new GameObject("skybox left face");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1),
                new Vector3(0, 90, 0), new Vector3(-halfWorldScale, 0, 0));
            texture = Content.Load<Texture2D>("Assets/Textures/Skybox/Tron/tron_lf");
            quad.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));
            sceneManager.ActiveScene.Add(quad);

            //skybox - right face
            quad = new GameObject("skybox right face");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1),
                new Vector3(0, -90, 0), new Vector3(halfWorldScale, 0, 0));
            texture = Content.Load<Texture2D>("Assets/Textures/Skybox/Tron/tron_rt");
            quad.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));
            sceneManager.ActiveScene.Add(quad);

            //skybox - top face
            quad = new GameObject("skybox top face");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1),
                new Vector3(90, -90, 0), new Vector3(0, halfWorldScale, 0));
            texture = Content.Load<Texture2D>("Assets/Textures/Skybox/Tron/tron_up");
            quad.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));
            sceneManager.ActiveScene.Add(quad);

            //skybox - front face
            quad = new GameObject("skybox front face");
            quad.Transform = new Transform(new Vector3(worldScale, worldScale, 1),
                new Vector3(0, -180, 0), new Vector3(0, 0, halfWorldScale));
            texture = Content.Load<Texture2D>("Assets/Textures/Skybox/Tron/tron_ft");
            quad.AddComponent(new Renderer(gdBasicEffect, new Material(texture, 1), quadMesh));
            sceneManager.ActiveScene.Add(quad);
        }

        #endregion Actions - Level Specific

        #region Actions - Engine Specific

        private void InitializeEngine(Vector2 resolution, bool isMouseVisible, bool isCursorLocked)
        {
            //add support for mouse etc
            InitializeInput();

            //add game effects
            InitializeEffects();

            //add camera, scene manager
            InitializeManagers();

            //share some core references
            InitializeGlobals();

            //set screen properties (incl mouse)
            InitializeScreen(resolution, isMouseVisible, isCursorLocked);

            //add game cameras
            InitializeCameras();
        }

        private void InitializeInput()
        {
            //Globally accessible inputs
            Input.Keys = new KeyboardComponent(this);
            Components.Add(Input.Keys);
            Input.Mouse = new MouseComponent(this);
            Components.Add(Input.Mouse);
            Input.Gamepad = new GamepadComponent(this);
            Components.Add(Input.Gamepad);
        }

        private void InitializeEffects()
        {
            //only for skybox with lighting disabled
            unlitEffect = new BasicEffect(_graphics.GraphicsDevice);
            unlitEffect.TextureEnabled = true;

            //all other drawn objects
            litEffect = new BasicEffect(_graphics.GraphicsDevice);
            litEffect.TextureEnabled = true;
            litEffect.LightingEnabled = true;
            litEffect.EnableDefaultLighting();
        }

        private void InitializeManagers()
        {
            //add event dispatcher for system events - the most important element!!!!!!
            eventDispatcher = new EventDispatcher(this);
            //add to Components otherwise no Update() called
            Components.Add(eventDispatcher);

            //add support for multiple cameras and camera switching
            cameraManager = new CameraManager(this);
            //add to Components otherwise no Update() called
            Components.Add(cameraManager);

            //big kahuna nr 1! this adds support to store, switch and Update() scene contents
            sceneManager = new SceneManager<Scene>(this);
            //add to Components otherwise no Update()
            Components.Add(sceneManager);

            //big kahuna nr 2! this renders the ActiveScene from the ActiveCamera perspective
            renderManager = new RenderManager(this, new ForwardSceneRenderer(_graphics.GraphicsDevice));
            renderManager.DrawOrder = 1;
            Components.Add(renderManager);

            //add support for playing sounds
            soundManager = new SoundManager();
            //why don't we add SoundManager to Components? Because it has no Update()
            //wait...SoundManager has no update? Yes, playing sounds is handled by an internal MonoGame thread - so we're off the hook!

            //add the physics manager update thread
            physicsManager = new PhysicsManager(this, AppData.GRAVITY);
            Components.Add(physicsManager);

            #region Collision - Picking

            //picking support using physics engine
            //this predicate lets us say ignore all the other collidable objects except interactables and consumables
            Predicate<GameObject> collisionPredicate =
                (collidableObject) =>
                {
                    if (collidableObject != null)
                        return collidableObject.GameObjectType
                        == GameObjectType.Interactable
                        || collidableObject.GameObjectType == GameObjectType.Consumable
                        || collidableObject.GameObjectType == GameObjectType.Collectible;
                    return false;
                };

            pickingManager = new PickingManager(this,
                AppData.PICKING_MIN_PICK_DISTANCE,
                AppData.PICKING_MAX_PICK_DISTANCE,
                collisionPredicate);
            Components.Add(pickingManager);

            #endregion

            #region Game State

            //add state manager for inventory and countdown
            stateManager = new MyStateManager(this, AppData.MAX_GAME_TIME_IN_MSECS);
            Components.Add(stateManager);

            #endregion

            #region UI

            uiManager = new SceneManager<Scene2D>(this);
            uiManager.StatusType = StatusType.Drawn | StatusType.Updated;
            uiManager.IsPausedOnPlay = false;
            Components.Add(uiManager);

            var uiRenderManager = new Render2DManager(this, _spriteBatch, uiManager);
            uiRenderManager.StatusType = StatusType.Drawn | StatusType.Updated;
            uiRenderManager.DrawOrder = 2;
            uiRenderManager.IsPausedOnPlay = false;
            Components.Add(uiRenderManager);


            #endregion

            #region Menu

            menuManager = new SceneManager<Scene2D>(this);
            menuManager.StatusType = StatusType.Updated;
            menuManager.IsPausedOnPlay = true;
            Components.Add(menuManager);

            var menuRenderManager = new Render2DManager(this, _spriteBatch, menuManager);
            menuRenderManager.StatusType = StatusType.Drawn;
            menuRenderManager.DrawOrder = 3;
            menuRenderManager.IsPausedOnPlay = true;
            Components.Add(menuRenderManager);

            #endregion
        }

        private void InitializeGlobals()
        {
            //Globally shared commonly accessed variables
            Application.Main = this;
            Application.GraphicsDeviceManager = _graphics;
            Application.GraphicsDevice = _graphics.GraphicsDevice;
            Application.Content = Content;

            //Add access to managers from anywhere in the code
            Application.CameraManager = cameraManager;
            Application.SceneManager = sceneManager;
            Application.SoundManager = soundManager;
            Application.PhysicsManager = physicsManager;
            Application.UISceneManager = uiManager;
            Application.MenuSceneManager = menuManager;
        }

        /// <summary>
        /// Sets game window dimensions and shows/hides the mouse
        /// </summary>
        /// <param name="resolution"></param>
        /// <param name="isMouseVisible"></param>
        /// <param name="isCursorLocked"></param>
        private void InitializeScreen(Vector2 resolution, bool isMouseVisible, bool isCursorLocked)
        {
            Screen screen = new Screen();

            //set resolution
            screen.Set(resolution, isMouseVisible, isCursorLocked);

            //set global for re-use by other entities
            Application.Screen = screen;

            //set starting mouse position i.e. set mouse in centre at startup
            Input.Mouse.Position = screen.ScreenCentre;

            ////calling set property
            //_graphics.PreferredBackBufferWidth = (int)resolution.X;
            //_graphics.PreferredBackBufferHeight = (int)resolution.Y;
            //IsMouseVisible = isMouseVisible;
            //_graphics.ApplyChanges();
        }

        private void InitializeDebug(bool showCollisionSkins = true)
        {
            //intialize the utility component
            var perfUtility = new PerfUtility(this, _spriteBatch,
                new Vector2(10, 10),
                new Vector2(0, 22));

            //set the font to be used
            var spriteFont = Content.Load<SpriteFont>("Assets/Fonts/Perf");

            //add components to the info list to add UI information
            float headingScale = 1f;
            float contentScale = 0.9f;
            perfUtility.infoList.Add(new TextInfo(_spriteBatch, spriteFont, "Performance ------------------------------", Color.Yellow, headingScale * Vector2.One));
            perfUtility.infoList.Add(new FPSInfo(_spriteBatch, spriteFont, "FPS:", Color.White, contentScale * Vector2.One));
            perfUtility.infoList.Add(new TextInfo(_spriteBatch, spriteFont, "Camera -----------------------------------", Color.Yellow, headingScale * Vector2.One));
            perfUtility.infoList.Add(new CameraNameInfo(_spriteBatch, spriteFont, "Name:", Color.White, contentScale * Vector2.One));

            var infoFunction = (Transform transform) =>
            {
                return transform.Translation.GetNewRounded(1).ToString();
            };

            perfUtility.infoList.Add(new TransformInfo(_spriteBatch, spriteFont, "Pos:", Color.White, contentScale * Vector2.One,
                ref Application.CameraManager.ActiveCamera.transform, infoFunction));

            infoFunction = (Transform transform) =>
            {
                return transform.Rotation.GetNewRounded(1).ToString();
            };

            perfUtility.infoList.Add(new TransformInfo(_spriteBatch, spriteFont, "Rot:", Color.White, contentScale * Vector2.One,
                ref Application.CameraManager.ActiveCamera.transform, infoFunction));

            perfUtility.infoList.Add(new TextInfo(_spriteBatch, spriteFont, "Object -----------------------------------", Color.Yellow, headingScale * Vector2.One));
            perfUtility.infoList.Add(new ObjectInfo(_spriteBatch, spriteFont, "Objects:", Color.White, contentScale * Vector2.One));
            perfUtility.infoList.Add(new TextInfo(_spriteBatch, spriteFont, "Hints -----------------------------------", Color.Yellow, headingScale * Vector2.One));
            perfUtility.infoList.Add(new TextInfo(_spriteBatch, spriteFont, "Use mouse scroll wheel to change security camera FOV, F1-F4 for camera switch", Color.White, contentScale * Vector2.One));
            perfUtility.infoList.Add(new TextInfo(_spriteBatch, spriteFont, "Use Up and Down arrow to see progress bar change", Color.White, contentScale * Vector2.One));

            //add to the component list otherwise it wont have its Update or Draw called!
            // perfUtility.StatusType = StatusType.Drawn | StatusType.Updated;
            perfUtility.DrawOrder = 3;
            Components.Add(perfUtility);

            if (showCollisionSkins)
            {
                var physicsDebugDrawer = new PhysicsDebugDrawer(this);
                physicsDebugDrawer.DrawOrder = 4;
                Components.Add(physicsDebugDrawer);
            }
        }

        #endregion Actions - Engine Specific

        #region Actions - Update, Draw

        protected override void Update(GameTime gameTime)
        {
            #region Menu On/Off with U/P

            if (Input.Keys.IsPressed(Keys.Escape))
                Exit();

            if (Input.Keys.WasJustPressed(Keys.P))
            {
                EventDispatcher.Raise(new EventData(EventCategoryType.Menu,
                    EventActionType.OnPause));
            }
            else if (Input.Keys.WasJustPressed(Keys.U))
            {
                EventDispatcher.Raise(new EventData(EventCategoryType.Menu,
                   EventActionType.OnPlay));
            }

            #endregion

            #region Camera switching
            bool hasbeenPressed = false;

            if (Input.Keys.IsPressed(Keys.Space) && hasbeenPressed == false)
            {
                InitializeDistanceMeter();
                cameraManager.SetActiveCamera(AppData.THIRD_PERSON_CAMERA_NAME);
                hasbeenPressed = true;
            }

            if (Input.Keys.IsPressed(Keys.Up) && hasbeenPressed == false && Application.CameraManager.ActiveCamera.transform.Translation.Z >= -1)
            {
                InitializeDistanceMeter();
                cameraManager.SetActiveCamera(AppData.THIRD_PERSON_CAMERA_NAME);
                hasbeenPressed = true;
            }

            else if (Input.Keys.IsPressed(Keys.F1))
                cameraManager.SetActiveCamera(AppData.FIRST_PERSON_CAMERA_NAME);
            else if (Input.Keys.IsPressed(Keys.F2))
                cameraManager.SetActiveCamera(AppData.SECURITY_CAMERA_NAME);
            else if (Input.Keys.IsPressed(Keys.F3))
                cameraManager.SetActiveCamera(AppData.CURVE_CAMERA_NAME);
            else if (Input.Keys.IsPressed(Keys.F4))
                cameraManager.SetActiveCamera(AppData.THIRD_PERSON_CAMERA_NAME);

            #endregion Camera switching

            #region Gamepad

            var thumbsL = Input.Gamepad.ThumbSticks(false);
            //   System.Diagnostics.Debug.WriteLine(thumbsL);

            var thumbsR = Input.Gamepad.ThumbSticks(false);
            //     System.Diagnostics.Debug.WriteLine(thumbsR);

            //    System.Diagnostics.Debug.WriteLine($"A: {Input.Gamepad.IsPressed(Buttons.A)}");

            #endregion Gamepad

            #region Demo - Raising events using GDEvent

            //if (Input.Keys.WasJustPressed(Keys.E))
            //    OnChanged.Invoke(this, null); //passing null for EventArgs but we'll make our own class MyEventArgs::EventArgs later

            #endregion

            //fixed a bug with components not getting Update called
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }

        #endregion Actions - Update, Draw
    }
}
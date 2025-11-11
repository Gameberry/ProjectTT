using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

namespace GameBerry
{
    public class SkeletonAnimationHandler : MonoBehaviour
    {
        [SerializeField] public SkeletonAnimation _skeletonAnimation;

        [SerializeField] private List<SpineModelAnimationData> _statesAndAnimation = new List<SpineModelAnimationData>();
        [SerializeField] private List<AnimationTransition> _transitions = new List<AnimationTransition>();
        [SerializeField] private List<AnimationEventParticle> _eventparticle = new List<AnimationEventParticle>();

        [SerializeField] private bool _awakeInit = false;

        public Dictionary<string, Spine.Animation> AnimationList_Dic = new Dictionary<string, Spine.Animation>();
        private Dictionary<CharacterState, string> _myAnimation = new Dictionary<CharacterState, string>();


        private int _animNumber;

        private MeshRenderer _meshRenderer;

        [System.Serializable]
        public class StateNameToAnimationReference
        {
            //public string stateName;
            //public @string animation;
            //public Spine.Animation animation;
            [SpineAnimation] public string stateName;
            public Spine.Animation animation;
        }

        [System.Serializable]
        public class AnimationTransition
        {
            [SpineAnimation] public string fromeName;
            public Spine.Animation from;
            [SpineAnimation] public string toName;
            public Spine.Animation to;
        }

        [System.Serializable]
        public class AnimationEventParticle
        {
            [SpineEvent] public string eventName;
            public List<ParticleSystem> particleSystem;
        }

        public System.Action<string, string> AnimationEvent;

        private void Awake()
        {
            if (_skeletonAnimation == null)
                _skeletonAnimation = GetComponent<SkeletonAnimation>();

            if (_meshRenderer == null)
                _meshRenderer = GetComponent<MeshRenderer>();

            _skeletonAnimation.state.Event += HandleEvent;
            _skeletonAnimation.state.Start += StartEvent;
            _skeletonAnimation.state.Complete += EndEvent;

            if (_awakeInit == true)
            {
                
                foreach (var entry in _statesAndAnimation)
                {
                    SkeletonData skeletonData = _skeletonAnimation.skeletonDataAsset.GetSkeletonData(true);
                    entry.animation = skeletonData != null ? skeletonData.FindAnimation(entry.stateName) : null;
                }

                foreach (var entry in _transitions)
                {
                    SkeletonData skeletonData = _skeletonAnimation.skeletonDataAsset.GetSkeletonData(true);

                    entry.from = skeletonData != null ? skeletonData.FindAnimation(entry.fromeName) : null;
                    entry.to = skeletonData != null ? skeletonData.FindAnimation(entry.toName) : null;
                }

                if (skinList.Count > 0)
                    RefreshAttachSkin();
            }

        }

        public void SetAnimNumber(int number)
        {
            _animNumber = number;
        }

        public void SetSpineModel(SpineModelData spineModelData)
        {
            if (spineModelData == null)
                return;

            _skeletonAnimation.skeletonDataAsset = spineModelData.SkeletonData;
            if (spineModelData.SkinList.Count > 0)
                _skeletonAnimation.initialSkinName = spineModelData.SkinList[0];
            else
                _skeletonAnimation.initialSkinName = "default";
            _skeletonAnimation.Initialize(true);

            _skeletonAnimation.state.Event += HandleEvent;
            _skeletonAnimation.state.Start += StartEvent;
            _skeletonAnimation.state.Complete += EndEvent;

            SkeletonData skeletonData = _skeletonAnimation.skeletonDataAsset.GetSkeletonData(true);
            _statesAndAnimation = spineModelData.AnimationList;

            AnimationList_Dic.Clear();
            _myAnimation.Clear();

            foreach (var pair in skeletonData.Animations)
            {
                if (AnimationList_Dic.ContainsKey(pair.Name) == false)
                    AnimationList_Dic.Add(pair.Name, pair);
            }

            foreach (var entry in _transitions)
            {

                entry.from = skeletonData != null ? skeletonData.FindAnimation(entry.fromeName) : null;
                entry.to = skeletonData != null ? skeletonData.FindAnimation(entry.toName) : null;
            }
        }

        public void SetOrderInLayer(int orderinlayer)
        {
            if (_meshRenderer != null)
                _meshRenderer.sortingOrder = orderinlayer;
        }

        //public void GetBounds()
        //{
        //    float x, y, width, height;
        //    float[] vertexBuffer = null;

        //    _skeletonAnimation.skeleton.GetBounds(out x, out y, out width, out height, ref vertexBuffer);

        //    Debug.Log(string.Format("X : {0}, Y : {1}, width : {2} height : {3}",
        //            x, y, width, height));

        //    //Debug.Log(string.Format("X : {0}, Y : {1}, ScaleX : {2} ScaleY : {3}",
        //    //    _skeletonAnimation.skeleton.X,
        //    //    _skeletonAnimation.skeleton.Y,
        //    //    _skeletonAnimation.skeleton.ScaleX,
        //    //    _skeletonAnimation.skeleton.ScaleY));
        //}

        private void HandleEvent(TrackEntry trackEntry, Spine.Event e)
        {
            string eventname = e.ToString();

            foreach (var eventparticle in _eventparticle)
            {
                if (eventparticle.eventName == eventname)
                {
                    if (eventparticle.particleSystem == null)
                        continue;

                    for (int i = 0; i < eventparticle.particleSystem.Count; ++i)
                    {
                        eventparticle.particleSystem[i].Stop();
                        eventparticle.particleSystem[i].Play();
                    }
                }
            }

            if (AnimationEvent != null)
                AnimationEvent(trackEntry.ToString(), eventname);
        }

        public void SetAnimationSpeed(float speed)
        {
            _skeletonAnimation.AnimationState.TimeScale = speed;
        }

        private void StartEvent(TrackEntry trackEntry)
        {
            if (AnimationEvent != null)
                AnimationEvent(trackEntry.ToString(), "Start");
        }

        private void EndEvent(TrackEntry trackEntry)
        {
            if (AnimationEvent != null)
                AnimationEvent(trackEntry.ToString(), "End");
        }

        public void SetColor(Color color)
        {
            if (_skeletonAnimation != null)
            {
                _skeletonAnimation.skeleton.SetColor(color);
            }
        }

        [SpineEvent] public string eventname;
        [SpineAnimation] public string testAniName;
        [SpineSkin] public string testSpineSkin;
        [ContextMenu("TestPlayAnimation")]
        public void TestPlayAnimation()
        {
            PlayAnimation_Once(testAniName, false);
        }

        [ContextMenu("TesChangeSkin")]
        public void TesChangeSkin()
        {
            SetSkin(testSpineSkin);
        }

        public void SetSkin(string skin)
        {
            if (_skeletonAnimation != null)
            {
                _skeletonAnimation.skeleton.SetSkin(skin);
                _skeletonAnimation.skeleton.SetSlotsToSetupPose();
            }
        }

        [SpineSkin]
        public List<string> skinList = new List<string>();

        Skin myEquipsSkin = new Skin("my new skin");

        public void AddAttachSkin(string attachSkin)
        {
            skinList.Add(attachSkin);
        }

        public void ReleaseAttachSkin()
        {
            skinList.Clear();
        }

        [ContextMenu("RefreshAttachSkin")]
        public void RefreshAttachSkin()
        {
            if (_skeletonAnimation != null)
            {
                Skeleton skeleton = _skeletonAnimation.skeleton;
                SkeletonData skeletonData = skeleton.Data;

                myEquipsSkin.Clear();

                for (int i = 0; i < skinList.Count; ++i)
                {
                    string skinname = skinList[i];

                    if (string.IsNullOrEmpty(skinname) == true)
                        continue;

                    myEquipsSkin.AddSkin(skeletonData.FindSkin(skinname));
                }

                _skeletonAnimation.skeleton.SetSkin(myEquipsSkin);
                _skeletonAnimation.skeleton.SetSlotsToSetupPose();
                _skeletonAnimation.skeleton.SetBonesToSetupPose();
                _skeletonAnimation.LateUpdate();

                _myAnimation.Clear();
            }
        }

        [ContextMenu("ResetAttachSkin")]
        private void ResetAttachSkin()
        {
            if (_skeletonAnimation != null)
            {
                Skeleton skeleton = _skeletonAnimation.skeleton;
                SkeletonData skeletonData = skeleton.Data;

                myEquipsSkin.Clear();
                _skeletonAnimation.skeleton.SetSkin(myEquipsSkin);
                _skeletonAnimation.skeleton.SetSlotsToSetupPose();
            }
        }

        private string GetAniClipName(CharacterState characterState)
        {
            if (_myAnimation.ContainsKey(characterState) == false)
            {
                SpineModelAnimationData spineModelAnimationData = _statesAndAnimation.Find(x => x.stateName == string.Format("{0}_{1}", characterState, _animNumber));
                if (spineModelAnimationData != null)
                    _myAnimation.Add(characterState, spineModelAnimationData.stateName);
                else
                    _myAnimation.Add(characterState, characterState.ToString());
            }

            return _myAnimation[characterState];
        }

        public void PlayAnimation(string stateShortName)
        {
            PlayAnimationForState(stateShortName, 0);
        }

        public void PlayAnimation_Once(CharacterState characterState, bool loop)
        {
            PlayAnimation_Once(GetAniClipName(characterState), loop);
        }

        public void PlayAnimation_Once(string stateShortName, bool loop)
        {
            //var foundAnimation = GetAnimationForState(StringToHash(stateShortName));
            //if (foundAnimation == null)
            //    return;

            if (AnimationList_Dic.ContainsKey(stateShortName) == false)
                return;

            _skeletonAnimation.AnimationState.SetAnimation(0, stateShortName, loop);
        }

        /// <summary>
        /// 2D 뒤집기 메서드
        /// </summary>
        /// <param name="horizontal"></param>
        public void SetFlip(float horizontal)
        {
            if (horizontal != 0)
            {
                _skeletonAnimation.skeleton.ScaleX = horizontal > 0 ? 1f : -1f;
            }
        }

        public void PlayAnimationForState(string stateShortName, int layerIndex)
        {
            PlayAnimationForState(StringToHash(stateShortName), layerIndex);
        }

        /// <summary>
        /// PlayAnimationForState Overloading 해당 애니메이션을 실행
        /// </summary>
        /// <param name="stateShortName">실행하고자 하는 애니메이션 이름</param>
        /// <param name="layerIndex">트랙/레이어 번호</param>
        public void PlayAnimationForState(int stateShortName, int layerIndex)
        {
            var foundAnimation = GetAnimationForState(stateShortName);
            if (foundAnimation == null)
                return;

            PlayNewAnimation(foundAnimation, layerIndex);
        }

        public Spine.Animation GetAnimationForState(string stateShortName)
        {
            return GetAnimationForState(StringToHash(stateShortName));
        }

        /// <summary>
        /// GetAnimationForState Overloading 해당 애니메이션을 반환(없다면 null)
        /// </summary>
        /// <param name="stateShortName">찾고자 하는 애니메이션 이름(정수로 들어옴)</param>
        /// <returns>해당 애니메이션</returns>
        public Spine.Animation GetAnimationForState(int stateShortName)
        {
            var foundState = _statesAndAnimation.Find(entry => StringToHash(entry.stateName) == stateShortName);
            return ((foundState == null) ? null : foundState.animation);
        }

        /// <summary>
        /// 애니메이션 재생 메서드
        /// 현재 진행중인 애니메이션이 없다면 || 전환 애니메이션이 없다면 바로 애니메이션 전환
        /// 있다면 전환 애니메이션 우선 재생 후 재생
        /// </summary>
        /// <param name="target"></param>
        /// <param name="layerIndex"></param>
        public void PlayNewAnimation(Spine.Animation target, int layerIndex)
        {
            Spine.Animation transition = null;
            Spine.Animation current = target;

            if (current != null)
                transition = TryGetTransition(current);

            if (transition != null)
            {
                _skeletonAnimation.AnimationState.SetAnimation(layerIndex, current, false);
                _skeletonAnimation.AnimationState.AddAnimation(layerIndex, transition, true, 0f);
            }
            else
            {
                _skeletonAnimation.AnimationState.SetAnimation(layerIndex, target, true);
            }
        }

        /// <summary>
        /// 현재 애니메이션에서 다음 애니메이션으로 전환될 때 전환 애니메이션이 있는지 판단
        /// </summary>
        /// <param name="from">현재 애니메이션</param>
        /// <param name="to">다음 애니메이션</param>
        /// <returns>없다면 null 있다면 전환애니메이션(ex)ldel-to-jump)</returns>
        private Spine.Animation TryGetTransition(Spine.Animation from)
        {
            foreach (var transition in _transitions)
            {
                if (transition.from == from && transition.to != null)
                {
                    return transition.to;
                }
            }

            return null;
        }

        /// <summary>
        /// 애니메이션 문자열을 해쉬값으로 반환
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private int StringToHash(string str)
        {
            return Animator.StringToHash(str);
        }
    }
}
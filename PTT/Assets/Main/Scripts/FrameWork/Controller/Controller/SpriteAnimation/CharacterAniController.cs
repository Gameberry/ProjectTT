using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameBerry
{
    /// <summary>
    /// 추후 Skin이 생길 때 CharacterAniSpritePainter에서 받은 데이터를 가지고 이미지를 선택해준다.
    /// </summary>
    public class CharacterAniController : MonoBehaviour
    {
        public CharacterControllerBase m_characterControllerBase;

        [SerializeField]
        private List<SpriteAniPart> m_charAniPart = new List<SpriteAniPart>();
        private Dictionary<AnimationPart, SpriteAniPart> m_charAniPart_Dic = new Dictionary<AnimationPart, SpriteAniPart>();

        public Dictionary<AnimationPart, SpriteAniPart> CharAniPart_Dic { get { return m_charAniPart_Dic; } }

        private LinkedList<SpriteAniPart> m_changedAniPart_List = new LinkedList<SpriteAniPart>();

        private List<AnimationPart> ChangedPart = new List<AnimationPart>();
        
        private Material m_myMaterial;

        private Transform m_charTransform;

        private CharacterAniFrameSelector m_frameSeleter;
        public CharacterAniFrameSelector FrameSeleter { get { return m_frameSeleter; } }

        private System.Action<AnimationAction> m_aniAction = null;

        private AnimationSpriteLibraryAsset m_animationSpriteLibraryAsset = null;

        private string m_variationNum = string.Empty;

        public WeaponParticleSortingOrder m_weaponEffect = null;

        //------------------------------------------------------------------------------------
        public void Init(Transform charroottrans)
        {
            m_frameSeleter = new CharacterAniFrameSelector();
            m_frameSeleter.Init(this);
            m_frameSeleter.ConnectAniActionCallBack(AniActionCallBack);

            m_charTransform = charroottrans;

            for (int i = 0; i < m_charAniPart.Count; ++i)
            {
                m_charAniPart_Dic.Add(m_charAniPart[i].PartID, m_charAniPart[i]);

                if (m_charAniPart[i].PartID == AnimationPart.Body)
                {
                    if (m_charAniPart[i].PartID == AnimationPart.Body)
                        m_myMaterial = m_charAniPart[i].Renderer.sharedMaterial;
                }
            }
        }
        //------------------------------------------------------------------------------------
        public List<SpriteAniPart> GetSpriteAniParts()
        {
            return m_charAniPart;
        }
        //------------------------------------------------------------------------------------
        public void SetAnimationSpriteLibrary()
        {
            //m_variationNum = m_characterControllerBase.VariationNumber.ToString();

            //m_animationSpriteLibraryAsset = Managers.AnimationSpriteManager.Instance.GetAnimationSpriteLibraryAsset(m_characterControllerBase.GroupIndex, m_characterControllerBase.VariationNumber);
        }
        //------------------------------------------------------------------------------------
        public Material GetMaterial()
        {
            return m_myMaterial;
        }
        //------------------------------------------------------------------------------------
        public void ConnectAniActionState(System.Action<AnimationAction> action)
        {
            m_aniAction = action;
        }
        //------------------------------------------------------------------------------------
        //public void CapyCharacterAniController(CharacterAniController capyCharacterAniController)
        //{
        //    m_frameSeleter.SetDummyCharacterAniFrameSelector(capyCharacterAniController.FrameSeleter);
        //}
        ////------------------------------------------------------------------------------------
        public void MoveToCharRoot(Vector3 pos)
        {
            if (m_charTransform != null)
            {
                Vector3 applypos = m_charTransform.transform.position;
                if (m_characterControllerBase != null)
                {
                    if(m_characterControllerBase.LookDirection == Enum_LookDirection.Left)
                        pos.x = pos.x * -1.0f;
                }

                applypos += pos;
                m_charTransform.transform.position = applypos;
            }
        }
        //------------------------------------------------------------------------------------
        public void PlayAnimation(CharacterState aniplaytype, string aniId = "")
        {
            if (m_frameSeleter != null)
                m_frameSeleter.PlayStateAnimation(aniplaytype, aniId);
        }
        //------------------------------------------------------------------------------------
        private void AniActionCallBack(AnimationAction aniaction)
        {
            if (m_aniAction != null)
                m_aniAction(aniaction);
        }
        //------------------------------------------------------------------------------------
        public void HideBodyOrderSprite()
        {
            for (int i = 0; i < m_charAniPart.Count; ++i)
            {
                if (m_charAniPart[i].PartID != AnimationPart.Body)
                    m_charAniPart[i].Renderer.sprite = null;
            }
        }
        //------------------------------------------------------------------------------------
        private void Update()
        {
            if (m_frameSeleter != null)
                m_frameSeleter.Updated();
        }
        //------------------------------------------------------------------------------------
        public void SetAnimationActionData(AnimationActionData actiondata)
        {
            MoveToCharRoot(actiondata.CharWorldPosition);
            SetAniFrameData(actiondata.FrameDatas);
        }
        //------------------------------------------------------------------------------------
        public void SetAniFrameData(List<AnimationFrameData> framedata)
        {
            //LinkedListNode<SpriteAniPart> node = m_changedAniPart_List.First;
            //LinkedListNode<SpriteAniPart> nextnode = null;

            //while (node != null)
            //{
            //    nextnode = node.Next;

            //    node.Value.Renderer.sprite = null;
            //    m_changedAniPart_List.Remove(node);

            //    node = nextnode;
            //}

            for (int i = 0; i < ChangedPart.Count; ++i)
            {
                if (m_charAniPart_Dic.ContainsKey(ChangedPart[i]) == true)
                    m_charAniPart_Dic[ChangedPart[i]].Renderer.sprite = null;
            }

            ChangedPart.Clear();

            bool viewWeapon = false;

            for (int i = 0; i < framedata.Count; ++i)
            {
                SpriteAniPart data = null;
                if (m_charAniPart_Dic.ContainsKey(framedata[i].PartID) == true)
                {
                    m_charAniPart_Dic.TryGetValue(framedata[i].PartID, out data);

                    if (data == null)
                        return;
                }
                else
                    continue;


                //int sortingOrder = framedata[i].OrderInLayer + m_characterControllerBase.AddSortingRenderer;

                if (m_animationSpriteLibraryAsset != null)
                    data.Renderer.sprite = m_animationSpriteLibraryAsset.GetSprite(framedata[i].SpriteName);

                if (data.Renderer.sprite == null)
                    continue;

                //data.Renderer.sortingOrder = sortingOrder;
                data.Renderer.transform.localPosition = framedata[i].LocalPosition;
                data.Renderer.transform.localEulerAngles = framedata[i].LocalRotation;
                data.Renderer.transform.localScale = framedata[i].LocalScale;

                if (string.IsNullOrEmpty(framedata[i].ParticleName) == false)
                {
                    ParticlePoolElement particlePoolElement = ParticleManager.Instance.GetParticle(framedata[i].ParticleBundleTag, framedata[i].ParticleName);

                    if (particlePoolElement != null)
                    {
                        if (framedata[i].ParticleWorldView == false)
                        {
                            particlePoolElement.transform.SetParent(data.Renderer.transform);
                            particlePoolElement.transform.localPosition = Vector3.zero;
                            particlePoolElement.transform.localEulerAngles = Vector3.zero;
                        }
                        else
                        {
                            particlePoolElement.transform.SetParent(null);
                            particlePoolElement.transform.position = data.Renderer.transform.position;
                            Vector3 rotate = Vector3.zero;
                            rotate.y = m_characterControllerBase.LookDirection == Enum_LookDirection.Left ? 180.0f : 0.0f;
                            particlePoolElement.transform.localEulerAngles = rotate;
                        }

                        particlePoolElement.gameObject.SetActive(true);
                        particlePoolElement.PlayParticle();

                        m_characterControllerBase.AddPlayParticle(particlePoolElement);
                    }
                }

                //m_changedAniPart_List.AddLast(data);
                ChangedPart.Add(data.PartID);
            }

            if (m_weaponEffect != null)
                m_weaponEffect.gameObject.SetActive(viewWeapon);
        }
        //------------------------------------------------------------------------------------
    }
}
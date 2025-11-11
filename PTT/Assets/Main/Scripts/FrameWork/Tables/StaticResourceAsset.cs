using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace GameBerry
{
    [System.Serializable]
    public class V2GradeColorData
    {
        public V2Enum_Grade v2Enum_Grade;
        [Header("-----------Text-----------")]
        public Color GradeTextColor = Color.white;
        public Sprite GradeTextImage;
        public bool UseGradeTextGradation = false;
        public TMPro.VertexGradient GradeTextColorGradient;

        [Header("-----------BG-----------")]
        public Color GradeColor = Color.white;
        public Sprite GradeBGImage;
        public bool UseGradeImageGradation = false;
        public Color Vertex1 = Color.white;
        public Color Vertex2 = Color.white;


        public Sprite GradeSprite;
    }

    [System.Serializable]
    public class VarianceColor
    {
        public HpMpVarianceType VarianceType = HpMpVarianceType.None;
        public Color color;
        public Material VarianceMaterial;
        public TMPro.TMP_ColorGradient VarianceColorGradient;
    }

    [System.Serializable]
    public class ElementFrameResource
    {
        public Sprite Frame_SS;
        public Sprite Frame_S;
        public Sprite Frame;

        public Transform Frame_Light_SS;
        public Transform Frame_Light_S;

        public Image m_allyFrame;


        public Sprite GradeFrame_SS;
        public Sprite GradeFrame_S;
        public Sprite GradeFrame;

        public Image m_GradeFrame;

        public void SetFrame(V2Enum_Grade v2Enum_Grade)
        {
            if (m_allyFrame != null)
            {
                if (v2Enum_Grade == V2Enum_Grade.SS || v2Enum_Grade == V2Enum_Grade.SR)
                {
                    if (m_allyFrame != null)
                        m_allyFrame.sprite = Frame_SS;

                    if (Frame_Light_SS != null)
                        Frame_Light_SS.gameObject.SetActive(true);

                    if (Frame_Light_S != null)
                        Frame_Light_S.gameObject.SetActive(false);

                    if (m_GradeFrame != null)
                        m_GradeFrame.sprite = GradeFrame_SS;
                }
                else if (v2Enum_Grade == V2Enum_Grade.S)
                {
                    if (m_allyFrame != null)
                        m_allyFrame.sprite = Frame_S;

                    if (Frame_Light_SS != null)
                        Frame_Light_SS.gameObject.SetActive(false);

                    if (Frame_Light_S != null)
                        Frame_Light_S.gameObject.SetActive(true);

                    if (m_GradeFrame != null)
                        m_GradeFrame.sprite = GradeFrame_S;
                }
                else
                {
                    if (m_allyFrame != null)
                        m_allyFrame.sprite = Frame;

                    if (Frame_Light_SS != null)
                        Frame_Light_SS.gameObject.SetActive(false);

                    if (Frame_Light_S != null)
                        Frame_Light_S.gameObject.SetActive(false);

                    if (m_GradeFrame != null)
                        m_GradeFrame.sprite = GradeFrame;
                }
            }
        }
    }

    [System.Serializable]
    public class GradeFrame
    {
        public V2Enum_Grade v2Enum_Grade;
        public Sprite Sprite;
    }

    [System.Serializable]
    public class ElementFrameResourceData
    {
        public List<GradeFrame> FrameSprite = new List<GradeFrame>();
        public Sprite DefaultFrame;

        public List<GradeFrame> AllyFrameSprite = new List<GradeFrame>();
        public Sprite DefaultAllyFrame;

        public List<GradeFrame> GradeFrameSprite = new List<GradeFrame>();
        public Sprite DefaultGradeFrame;

        public void SetFrame(V2Enum_Grade v2Enum_Grade, Image frame)
        {
            if (frame == null)
                return;

            GradeFrame gradeFrame = FrameSprite.Find(x => x.v2Enum_Grade == v2Enum_Grade);
            if (gradeFrame == null)
                frame.sprite = DefaultFrame;
            else
                frame.sprite = gradeFrame.Sprite;
        }

        public void SetAllyFrame(V2Enum_Grade v2Enum_Grade, Image frame)
        {
            if (frame == null)
                return;

            GradeFrame gradeFrame = AllyFrameSprite.Find(x => x.v2Enum_Grade == v2Enum_Grade);
            if (gradeFrame == null)
                frame.sprite = DefaultAllyFrame;
            else
                frame.sprite = gradeFrame.Sprite;
        }

        public void SetGradeFrame(V2Enum_Grade v2Enum_Grade, Image frame)
        {
            if (frame == null)
                return;

            GradeFrame gradeFrame = GradeFrameSprite.Find(x => x.v2Enum_Grade == v2Enum_Grade);
            if (gradeFrame == null)
                frame.sprite = DefaultGradeFrame;
            else
                frame.sprite = gradeFrame.Sprite;
        }
    }

    [System.Serializable]
    public class GambleCardSprite
    {
        public V2Enum_ARR_SynergyType CardType;

        public Sprite SynergyIcon;

        public Color SpotBar;
        public Color Bar;

        public Color CircleBg;

        public Sprite JobGearIcon;
        public Sprite JobRuneIcon;
    }

    [System.Serializable]
    public class GambleGradeBGSprite
    {
        public V2Enum_Grade GradeType;

        public Sprite GradeSprite;
    }

    [System.Serializable]
    public class TotalLevelEffectColorData
    {
        public V2Enum_ARRR_TotalLevelType EffectType;
        public Color EffectColor = Color.white;
    }

    [System.Serializable]
    public class GearResourceData
    {
        public V2Enum_GearType v2Enum_GearType;
        public Sprite GearSprite;
        public string gearLocalKey;
    }

    [CreateAssetMenu(fileName = "StaticResource", menuName = "Table/StaticResource", order = 1)]
    public class StaticResourceAsset : ScriptableObject
    {
        public List<GambleCardSprite> GambleCardSprites = new List<GambleCardSprite>();

        public List<GambleGradeBGSprite> GambleGradeBGSprites = new List<GambleGradeBGSprite>();

        public ElementFrameResourceData elementFrameResourceData;

        public List<V2GradeColorData> GradeColorDatas = new List<V2GradeColorData>();

        public List<VarianceColor> m_varianceColor_List = new List<VarianceColor>();

        public List<TotalLevelEffectColorData> TotalLevelEffectColor = new List<TotalLevelEffectColorData>();

        public List<GearResourceData> GearResourceDatas = new List<GearResourceData>();

        public Gradient HitColorGradient;
        public float HitRecoveryTime = 0.1f;

        public Gradient DeadColorGradient;
        public float DeadDirectionTime = 0.5f;

        public float DeadDirectionSwingForce = 1.0f;
        public float DeadDirectionSwingRotationSpeed = 1.0f;

    }
}

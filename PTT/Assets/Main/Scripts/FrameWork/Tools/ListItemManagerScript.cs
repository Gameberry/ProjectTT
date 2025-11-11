using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GameBerry.UI
{
    public class ListItemManagerScript : MonoBehaviour
    {
        [Header("Margin")]
        public bool autoMargin = true;
        public Vector2 inputMargin;

        [Header("Count")]
        public int inputWidthCount, inputHeightCount;


        [Header("Size")]
        public int itemCount = 100;

        public float nextLineRatio = 0.5f;

        public int cheatitemcount = 10;

        ScrollRect scrollRect;

        GameObject targetContent;
        RectTransform targetContentRecttrans;
        Vector2 viewSize, itemSize;
        Vector2 margin;
        int widthCount, heightCount;

        DirectionStack<ListItemParentScript> myScrollContents;

        float topBound, botBound;
        int topId, botId;

        public float xStartPos = 0.0f;

        //------------------------------------------------------------------------------------
        public List<T> Init<T>(T itemPrefab) where T : ListItemParentScript
        {
            myScrollContents = new DirectionStack<ListItemParentScript>();
            scrollRect = GetComponent<ScrollRect>();
            if (scrollRect != null)
                scrollRect.onValueChanged.AddListener(OnValueChange);

            targetContent = scrollRect.content.gameObject;
            targetContentRecttrans = targetContent.GetComponent<RectTransform>();

            viewSize = GetComponent<RectTransform>().sizeDelta;
            itemSize = itemPrefab.GetComponent<RectTransform>().sizeDelta;

            if (autoMargin)
            {

                widthCount = (int)(viewSize.x / itemSize.x);
                heightCount = (int)(viewSize.y / itemSize.y);

                // 아이템 사이 여백은 아이템 개수 + 1 개 존재한다.
                margin.x = (viewSize.x - itemSize.x * widthCount) / (float)(widthCount + 1) + itemSize.x;
                margin.y = (viewSize.y - itemSize.y * heightCount) / (float)(heightCount + 1) + itemSize.y;
            }
            else
            {
                widthCount = inputWidthCount;
                heightCount = inputHeightCount;
                margin = inputMargin;
            }

            Vector2 contentSize = targetContent.GetComponent<RectTransform>().sizeDelta;
            contentSize.y = margin.y * (Mathf.Ceil(itemCount / widthCount) + 1) - itemSize.y;
            targetContent.GetComponent<RectTransform>().sizeDelta = contentSize;

            inputMargin = margin;

            int tempId = 0;

            bool oddNum = widthCount % 2 == 0 ? false : true;

            xStartPos = margin.x;
            if (oddNum == false)
                xStartPos += (margin.x * 0.5f);

            xStartPos *= -1.0f;

            topId = widthCount * -1;
            botId = tempId - 1;

            tempId = topId;

            List<T> createElement = new List<T>();

            // 세로 방향으로 위, 아래 (스크롤 감안) 2개 더 생성.
            for (int i = 0; i < heightCount + 2; i++)
            {
                for (int j = 0; j < widthCount; j++)
                {
                    GameObject contentObj = Instantiate(itemPrefab.gameObject, targetContent.transform);
                    Vector2 listPos = new Vector2(xStartPos + (MathDatas.Abs(tempId % widthCount) * margin.x), itemSize.y / 2 - margin.y * (tempId / widthCount + 1));
                    contentObj.transform.localPosition = listPos;

                    T listItemParentScript = contentObj.GetComponent<T>();
                    listItemParentScript.SetId(tempId);

                    createElement.Add(listItemParentScript);

                    myScrollContents.PushBack(listItemParentScript);

                    if (tempId < 0 || tempId >= itemCount)
                    {
                        contentObj.SetActive(false);
                    }
                    else
                    {
                        contentObj.SetActive(true);
                    }

                    tempId++;
                }
            }

            //topId = -1;
            //botId = tempId - 1;

            botId = tempId;

            topBound = 0f;
            botBound = -viewSize.y;

            return createElement;
        }
        //------------------------------------------------------------------------------------
        public void ChangeDataCount(int dataCount)
        {
            itemCount = dataCount;

            Vector2 contentSize = targetContentRecttrans.sizeDelta;
            contentSize.y = margin.y * (Mathf.Ceil(itemCount / widthCount) + 1) - itemSize.y;
            targetContentRecttrans.sizeDelta = contentSize;

            Vector3 contentpos = scrollRect.content.transform.localPosition;
            contentpos.y = 0.0f;
            scrollRect.content.transform.localPosition = contentpos;

            int tempId = 0;

            topId = widthCount * -1;
            botId = tempId - 1;

            tempId = topId;

            int elementcount = myScrollContents.Count();

            for (int i = 0; i < elementcount; ++i)
            {
                ListItemParentScript contentObj = myScrollContents.PopFront();
                Vector2 listPos = new Vector2(xStartPos + (MathDatas.Abs(tempId % widthCount) * margin.x), itemSize.y / 2 - margin.y * (tempId / widthCount + 1));
                contentObj.transform.localPosition = listPos;
                contentObj.SetId(tempId);
                myScrollContents.PushBack(contentObj);

                if (tempId < 0 || tempId >= itemCount)
                {
                    contentObj.gameObject.SetActive(false);
                }
                else
                {
                    contentObj.gameObject.SetActive(true);
                }

                tempId++;
            }

            botId = tempId;

            topBound = 0f;
            botBound = -viewSize.y;
        }
        //------------------------------------------------------------------------------------
        private void OnValueChange(Vector2 vector2)
        {
            if (vector2.y < 0.0f || vector2.y > 1.0f)
                return;

            //Updated();
        }
        //------------------------------------------------------------------------------------
        private void SetElementPos()
        {
            int intY = -(int)scrollRect.content.transform.localPosition.y;
            while (intY > topBound + (margin.y * nextLineRatio))
            {
                // 콘텐츠 위 부분 (아래로 스크롤)

                for (int i = 0; i < widthCount; ++i)
                {
                    topId--;
                    botId--;

                    ListItemParentScript botTarget = myScrollContents.PopBack();

                    if (topId < 0 || topId >= itemCount)
                    {
                        botTarget.gameObject.SetActive(false);
                    }
                    else
                    {
                        botTarget.gameObject.SetActive(true);
                    }

                    botTarget.SetId(topId);
                    Vector2 tempPosition = new Vector2(xStartPos + (MathDatas.Abs(topId % widthCount) * margin.x), itemSize.y / 2 - margin.y * (topId / widthCount + 1));
                    botTarget.transform.localPosition = tempPosition;

                    myScrollContents.PushFront(botTarget);
                }


                topBound += margin.y;
                botBound += margin.y;
            }

            while (intY - viewSize.y < botBound - (margin.y * nextLineRatio))
            {
                // 콘텐츠 아래 부분 (위로 스크롤)

                for (int i = 0; i < widthCount; ++i)
                {
                    topId++;
                    botId++;

                    ListItemParentScript botTarget = myScrollContents.PopFront();

                    if (botId < 0 || botId >= itemCount)
                    {
                        botTarget.gameObject.SetActive(false);
                    }
                    else
                    {
                        botTarget.gameObject.SetActive(true);
                    }

                    botTarget.SetId(botId);
                    Vector2 tempPosition = new Vector2(xStartPos + (MathDatas.Abs(botId % widthCount) * margin.x), itemSize.y / 2 - margin.y * (botId / widthCount));
                    botTarget.transform.localPosition = tempPosition;

                    myScrollContents.PushBack(botTarget);
                }


                topBound -= margin.y;
                botBound -= margin.y;
            }
        }
        //------------------------------------------------------------------------------------
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.I))
            {
                ChangeDataCount(cheatitemcount);
            }

            SetElementPos();
        }
        //------------------------------------------------------------------------------------
    }
}


// Copyright, jysa000@naver.com - 댄싱돌핀
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IAPTest : MonoBehaviour
{
    [SerializeField]
    private Button m_TestButton;

    [SerializeField]
    private Button m_frees;

    [SerializeField]
    private Button m_showProductList;

    [SerializeField]
    private Text m_showVersion;

    //Your products IDs. They should match the ids of your products in your store.
    private string goldProductId = "test1000";
    private string goldProductId1 = "dk_packageadfree01_5500";
    void Start()
    {
        InitializePurchasing();
    }

    void InitializePurchasing()
    {
        if (m_TestButton != null)
            m_TestButton.onClick.AddListener(Buytest1000Gold);

        if (m_frees != null)
            m_frees.onClick.AddListener(Buypackageadfree01Gold);

        if (m_showProductList != null)
            m_showProductList.onClick.AddListener(ShowProductList);

        if (m_showVersion != null)
            m_showVersion.text = Project.version;
    }

    public void Buytest1000Gold()
    {
        Debug.LogError("Click_test1000Buy");
        //GamePot.purchase(goldProductId);
    }

    public void Buypackageadfree01Gold()
    {
        Debug.LogError("Click_packageadfree01");
        //GamePot.purchase(goldProductId1);
    }

    public void ShowProductList()
    {
        //GamePot.getPurchaseDetailListAsync((bool success, NPurchaseItem[] purchaseInfoList, NError error) =>
        //{
        //    if (success)
        //    {
        //        NPurchaseItem[] items = purchaseInfoList;
        //        foreach (NPurchaseItem item in items)
        //        {
        //            Debug.LogError(item.productId);        // 상품ID
        //            Debug.Log(item.price);            // 가격
        //            Debug.Log(item.title);            // 제목
        //            Debug.Log(item.description);    // 설명
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("getPurchaseDetailListAsync Error");
        //        // API 오류 
        //        // result = error.ToJson();
        //    }
        //});
    }
}

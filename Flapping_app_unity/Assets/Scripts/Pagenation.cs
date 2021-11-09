using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Pagenation : MonoBehaviour
{
    [SerializeField] PictureProvider pictureController;
    [SerializeField] GameObject p;

    public void SetPagenation(int page, int lastPage, GameObject prev, GameObject next)
	{
        foreach (Transform n in transform) Destroy(n.gameObject);
        if (lastPage >= 5)
		{
            int min = page <= 2 ? 1 : page >= lastPage - 2 ? lastPage - 4 : page - 2;
            for (int i = 0; i < 5; i++)
            {
                var pi = Instantiate(p, transform);
                var rt = pi.GetComponent<RectTransform>();
                rt.anchoredPosition += Vector2.right * 80 * (i - 2);
                var pageNum = i + min;
                pi.GetComponentInChildren<Text>().text = pageNum.ToString();
				if (pageNum == page)
				{
                    pi.GetComponentInChildren<Text>().color = Color.yellow;
                }
                else
                {
                    pi.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        pictureController.SetPage(pageNum);
                    });
                }
            }
        }
		else
		{
            for (int i = 0; i < lastPage; i++)
            {
                var pi = Instantiate(p, transform);
                var rt = pi.GetComponent<RectTransform>();
                rt.anchoredPosition += Vector2.right * (rt.rect.width + 5) * (i - lastPage / 2f + 0.5f);
                var pageNum = i + 1;
                pi.GetComponentInChildren<Text>().text = pageNum.ToString();
                if (pageNum == page)
                {
                    pi.GetComponentInChildren<Text>().color = Color.yellow;
                }
                else
                {
                    pi.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        Debug.Log(pageNum); pictureController.SetPage(pageNum);
                    });
                }
            }
        }
        prev.SetActive(page != 1);
        next.SetActive(page != lastPage);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

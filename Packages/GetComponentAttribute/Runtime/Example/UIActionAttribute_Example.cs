#region Header
/*	============================================
 *	Author 			    	: Require PlayerPref Key : "Author"
 *	Initial Creation Date 	: 2020-07-14
 *	Summary 		        : 
 *  Template 		        : New Behaviour For Unity Editor V2
   ============================================ */
#endregion Header

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
public class UIActionAttribute_Example : MonoBehaviour
{
    /* const & readonly declaration             */

    /* enum & struct declaration                */

    public enum EButtonName
    {
        ButtonAttributeExample,
    }

    /* public - Field declaration               */


    /* protected & private - Field declaration  */


    // ========================================================================== //

    /* public - [Do~Something] Function 	        */

    //[UIButtonCall(EButtonName.ButtonAttributeExample)]
    //public void ButtonAttributeTest(Button pButton)
    //{
    //    Debug.Log(nameof(ButtonAttributeTest), this);
    //    StartCoroutine(ButtonCoroutine(pButton));
    //}

    // ========================================================================== //

    /* protected - [Override & Unity API]       */

    private void Awake()
    {
        UIActionAttributeSetter.DoSet_UIActionAttribute(this);
    }

    /* protected - [abstract & virtual]         */


    // ========================================================================== //

    #region Private

    IEnumerator ButtonCoroutine(Button pButton)
    {
        Text pText = pButton.GetComponentInChildren<Text>();

        pText.text = "Clicked!";

        yield return new WaitForSeconds(1f);

        pText.text = "Normal";

        yield break;
    }

    #endregion Private
}

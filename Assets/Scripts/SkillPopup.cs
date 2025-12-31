using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class SkillPopup : MonoBehaviour
{
    public static SkillPopup I;
    private RectTransform rectTransform;
    public Skill currentSkill;
    public Vector2 offset = new Vector2(0, 10f);
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI lvlText;
    public LocalizedString maxedString;
    public LocalizedString cheaperAvailableString;
    public GameObject priceIcon;
    
    private void Awake()
    {
        I = this;
        rectTransform = GetComponent<RectTransform>();
    }

    public void Show(Skill skill)
    {
        if (currentSkill == skill) return;
        if (currentSkill != null)
        {
            currentSkill.OnDeselect(null);
        }
        currentSkill = skill;
        rectTransform.gameObject.SetActive(true);
        rectTransform.SetAsLastSibling();
        rectTransform.anchoredPosition = skill.rectTransform.anchoredPosition + offset;
        
        skill.skillName.StringChanged += OnSkillNameChanged;
        OnSkillNameChanged(skill.skillName.GetLocalizedString());
        
        if (skill.description.IsEmpty) 
        {
            descriptionText.gameObject.SetActive(false); 
        }
        else
        {
            descriptionText.gameObject.SetActive(true);
            skill.description.StringChanged += OnDescriptionChanged;
            OnDescriptionChanged(skill.description.GetLocalizedString());
        }
    }
    
    public void Hide(Skill skill)
    {
        if (skill != null && currentSkill != skill) return;
        currentSkill = null;
        rectTransform.gameObject.SetActive(false);
        if (skill == null)
        {
            return;
        }
        skill.skillName.StringChanged -= OnSkillNameChanged;
        skill.description.StringChanged -= OnDescriptionChanged;
    }
    
    private void Update()
    {
        if (currentSkill == null) return;
        
        lvlText.text = currentSkill.currentLevel + "/" + currentSkill.MaxLevel;
        priceText.text = currentSkill.currentLevel < currentSkill.MaxLevel ?
            currentSkill.prices[currentSkill.currentLevel].ToString() : maxedString.GetLocalizedString();
        priceIcon.SetActive(currentSkill.currentLevel < currentSkill.prices.Count);
        priceText.rectTransform.sizeDelta = priceText.GetPreferredValues(priceText.text);
    }
    
    private void OnSkillNameChanged(string str)
    {
        skillNameText.text = ProcessString(str);
    }
    
    private void OnDescriptionChanged(string str)
    {
        if (str.Trim().Length == 0)
        {
            descriptionText.gameObject.SetActive(false);
            return;
        }
        descriptionText.gameObject.SetActive(true);
        descriptionText.text = ProcessString(str);
    }
    
    private string ProcessString(string str)
    {
        if (str.Contains("%d"))
        {
            str = str.Replace("%d", currentSkill.GetByText());
        }
        return str;
    }
}
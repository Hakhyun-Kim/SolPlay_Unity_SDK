using System;
using Frictionless;
using GLTFast;
using SolPlay.Scripts.Services;
using SolPlay.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SolPlay.Scripts.Ui
{
    /// <summary>
    /// Show the image and the power level of a given Nft and can have a click handler
    /// </summary>
    public class NftItemView : MonoBehaviour
    {
        public SolPlayNft CurrentSolPlayNft;
        public RawImage Icon;
        public TextMeshProUGUI Headline;
        public TextMeshProUGUI Description;
        public TextMeshProUGUI PowerLevel;
        public Button Button;
        public GameObject SelectionGameObject;
        public GameObject GltfRoot;
        public GltfAsset GltfAsset;
        public RenderTexture RenderTexture;
        public Camera Camera;
        public int RenderTextureSize = 75;
        
        private Action<NftItemView> onButtonClickedAction;

        public async void SetData(SolPlayNft solPlayNft, Action<NftItemView> onButtonClicked)
        {
            CurrentSolPlayNft = solPlayNft;
            Icon.gameObject.SetActive(false);
            GltfRoot.SetActive(false);
            
            if (gameObject.activeInHierarchy)
            {
                if (!string.IsNullOrEmpty(solPlayNft.MetaplexData.data.json.animation_url))
                {
                    Icon.gameObject.SetActive(true);
                    GltfRoot.SetActive(true);
                    RenderTexture = new RenderTexture(RenderTextureSize, RenderTextureSize, 1);
                    Camera.targetTexture = RenderTexture;
                    Camera.cullingMask = (1 << 19);
                    Icon.texture = RenderTexture;
                    var isLoaded = await GltfAsset.Load(solPlayNft.MetaplexData.data.json.animation_url);
                    if (isLoaded)
                    {
                        if (!GltfAsset)
                        {
                            // In case it was destroyed while loading
                            return;
                        }
                        LayerUtils.SetGameLayerRecursive(GltfAsset.gameObject, 19);
                    }
                    
                } else if (solPlayNft.MetaplexData.nftImage != null)
                {
                    Icon.gameObject.SetActive(true);
                    Icon.texture = solPlayNft.MetaplexData.nftImage.file;
                }
            }
            var nftService = ServiceFactory.Resolve<NftService>();

            SelectionGameObject.gameObject.SetActive(nftService.IsNftSelected(solPlayNft));
            Headline.text = solPlayNft.MetaplexData.data.name;
            Description.text = solPlayNft.MetaplexData.data.json.description;
            var nftPowerLevelService = ServiceFactory.Resolve<HighscoreService>();
            var highscoreForPubkey = nftPowerLevelService.GetHighscoreForPubkey(solPlayNft.MetaplexData.mint);
            if (highscoreForPubkey != null)
            {
                PowerLevel.text =
                    $"Score: {highscoreForPubkey.Highscore}";
            }
            else
            {
                PowerLevel.text = "Loading";
            }

            Button.onClick.AddListener(OnButtonClicked);
            onButtonClickedAction = onButtonClicked;
        }
        
        private void OnButtonClicked()
        {
            onButtonClickedAction?.Invoke(this);
        }
    }
}
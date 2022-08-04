using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowCounter {
    public class InfoPanel {
        private static Dictionary<string, GameObject> arrowCountSessionTexts = new Dictionary<string, GameObject>();
        private static Dictionary<string, GameObject> arrowCountTotalTexts = new Dictionary<string, GameObject>();

        public bool isActive { get; set; } = false;

        public static GameObject infoPanel { get; set; }
        public static GameObject infoPrefab;

        private static Dictionary<string, int> sessionArrowCounts { get; } = new Dictionary<string, int>();
        private static Dictionary<string, int> totalArrowCounts { get; } = new Dictionary<string, int>();

        private static List<string> arrowNames = new List<string> {
                    "$item_arrow_wood",
                    "$item_arrow_fire",
                    "$item_arrow_flint",
                    "$item_arrow_bronze",
                    "$item_arrow_iron",
                    "$item_arrow_silver",
                    "$item_arrow_obsidian",
                    "$item_arrow_poison",
                    "$item_arrow_frost",
                    "$item_arrow_needle"
                };

        public InfoPanel() {
            
        }

        public bool Toggle() {
            isActive = !isActive;
            infoPanel.SetActive(isActive);
            return isActive;
        }

        public void Update() {
            foreach (string arrowName in arrowNames) {
                arrowCountSessionTexts[arrowName].GetComponent<Text>().text = sessionArrowCounts[arrowName].ToString();
                arrowCountTotalTexts[arrowName].GetComponent<Text>().text = totalArrowCounts[arrowName].ToString();
            }
        }

        public void AddToTotal() {
            foreach (string arrowName in arrowNames) {
                totalArrowCounts[arrowName] += sessionArrowCounts[arrowName];
                sessionArrowCounts[arrowName] = 0;
            }
            Update();
        }

        public void Write(StreamWriter file) {
            foreach (KeyValuePair<string, int> entry in sessionArrowCounts) {
                file.WriteLine(entry.Key + "," + (entry.Value + totalArrowCounts[entry.Key]).ToString());
            }
        }

        public void SetZeroArrowCounts() {
            foreach (string arrowName in arrowNames) {
                sessionArrowCounts.Add(arrowName, 0);
                totalArrowCounts.Add(arrowName, 0);
            }
        }

        public void InitializeCountsForPlayer(string[] lines) {
            foreach (string line in lines) {
                totalArrowCounts[line.Split(',')[0]] = int.Parse(line.Split(',')[1]);
            }
        }

        public void Count(string ammoName) {
            sessionArrowCounts[ammoName]++;
        }

        public int GetSessionCount(string ammoName) {
            return sessionArrowCounts[ammoName];
        }

        public int GetTotalCount(string ammoName) {
            return totalArrowCounts[ammoName];
        }

        public void Deactivate() {
            isActive = false;
            infoPanel.SetActive(isActive);
        }

        public void Build() {
        infoPanel = GUIManager.Instance.CreateWoodpanel(
            parent: GUIManager.CustomGUIFront.transform,
            anchorMin: new Vector2(0.96f, 0.5f),
            anchorMax: new Vector2(0.96f, 0.5f),
            position: new Vector2(0, 0),
            width: 150,
            height: 500,
            draggable: false);

        float dataHeight = -35f - 45f;

        GameObject titleObject = GUIManager.Instance.CreateText(
            text: "Arrow Counter",
            parent: infoPanel.transform,
            anchorMin: new Vector2(0.5f, 1f),
            anchorMax: new Vector2(0.5f, 1f),
            position: new Vector2(5f, -45f),
            font: GUIManager.Instance.AveriaSerifBold,
            fontSize: 18,
            color: GUIManager.Instance.ValheimOrange,
            outline: true,
            outlineColor: Color.black,
            width: 135f,
            height: 45f,
            addContentSizeFitter: false);


        GameObject header1Object = GUIManager.Instance.CreateText(
            text: "Today's Usage",
            parent: infoPanel.transform,
            anchorMin: new Vector2(0.5f, 1f),
            anchorMax: new Vector2(0.5f, 1f),
            position: new Vector2(25f, -75f),
            font: GUIManager.Instance.AveriaSerifBold,
            fontSize: 14,
            color: GUIManager.Instance.ValheimOrange,
            outline: true,
            outlineColor: Color.black,
            width: 135f,
            height: 45f,
            addContentSizeFitter: false);

        // Create 
        foreach (string arrowName in arrowNames) {
            GameObject arrowNameObject = GUIManager.Instance.CreateText(
            text: arrowName.Split('_').Last().First().ToString().ToUpper() + String.Join("", arrowName.Split('_').Last().Skip(1)),
            parent: infoPanel.transform,
            anchorMin: new Vector2(0.05f, 1f),
            anchorMax: new Vector2(0.5f, 1f),
            position: new Vector2(35f, dataHeight),
            font: GUIManager.Instance.AveriaSerifBold,
            fontSize: 12,
            color: GUIManager.Instance.ValheimOrange,
            outline: true,
            outlineColor: Color.black,
            width: 50f,
            height: 15f,
            addContentSizeFitter: false);

            GameObject countTextObject = GUIManager.Instance.CreateText(
            text: sessionArrowCounts[arrowName].ToString(),
            parent: infoPanel.transform,
            anchorMin: new Vector2(0.05f, 1f),
            anchorMax: new Vector2(0.5f, 1f),
            position: new Vector2(130f, dataHeight),
            font: GUIManager.Instance.AveriaSerifBold,
            fontSize: 12,
            color: GUIManager.Instance.ValheimOrange,
            outline: true,
            outlineColor: Color.black,
            width: 50f,
            height: 15f,
            addContentSizeFitter: false);

            arrowCountSessionTexts[arrowName] = countTextObject;

            dataHeight -= 15;
        }

        dataHeight -= 30;
        GameObject header2Object = GUIManager.Instance.CreateText(
            text: "Total Usage",
            parent: infoPanel.transform,
            anchorMin: new Vector2(0.5f, 1f),
            anchorMax: new Vector2(0.5f, 1f),
            position: new Vector2(25f, dataHeight),
            font: GUIManager.Instance.AveriaSerifBold,
            fontSize: 14,
            color: GUIManager.Instance.ValheimOrange,
            outline: true,
            outlineColor: Color.black,
            width: 135f,
            height: 45f,
            addContentSizeFitter: false);

        foreach (string arrowName in arrowNames) {
            GameObject arrowNameObject = GUIManager.Instance.CreateText(
            text: arrowName.Split('_').Last().First().ToString().ToUpper() + String.Join("", arrowName.Split('_').Last().Skip(1)),
            parent: infoPanel.transform,
            anchorMin: new Vector2(0.05f, 1f),
            anchorMax: new Vector2(0.5f, 1f),
            position: new Vector2(35f, dataHeight),
            font: GUIManager.Instance.AveriaSerifBold,
            fontSize: 12,
            color: GUIManager.Instance.ValheimOrange,
            outline: true,
            outlineColor: Color.black,
            width: 50f,
            height: 15f,
            addContentSizeFitter: false);

            GameObject countTextObject = GUIManager.Instance.CreateText(
            text: totalArrowCounts[arrowName].ToString(),
            parent: infoPanel.transform,
            anchorMin: new Vector2(0.05f, 1f),
            anchorMax: new Vector2(0.5f, 1f),
            position: new Vector2(130f, dataHeight),
            font: GUIManager.Instance.AveriaSerifBold,
            fontSize: 12,
            color: GUIManager.Instance.ValheimOrange,
            outline: true,
            outlineColor: Color.black,
            width: 50f,
            height: 15f,
            addContentSizeFitter: false);

            arrowCountTotalTexts[arrowName] = countTextObject;

            dataHeight -= 15;
        }
        dataHeight -= 30;
        GameObject buttonObject = GUIManager.Instance.CreateButton(
        text: "Add to Total",
        parent: infoPanel.transform,
        anchorMin: new Vector2(0.5f, 1f),
        anchorMax: new Vector2(0.5f, 1f),
        position: new Vector2(0, dataHeight),
        width: 120f,
        height: 60f);
        buttonObject.SetActive(true);

        // Add a listener to the button to close the panel again
        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(AddToTotal);

        infoPanel.SetActive(false);
        }
    }
}

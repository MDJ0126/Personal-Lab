using Map.PathFinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Map.Sample
{
    public class Map : MonoBehaviour
    {
        private static Map _instance = null;
        public static Map Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<Map>();
                return _instance;
            }
        }

        public Player player;
        public MapCreater mapCreater;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // 벽 만들기
                var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (Physics.Raycast(mouseWorldPosition, transform.forward, out RaycastHit hit, 100f))
                {
                    var block = hit.transform.gameObject.GetComponent<BlockItem>();
                    if (block != null)
                    {
                        block.SetWall();
                    }
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                // 플레이어 이동하기
                var mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (Physics.Raycast(mouseWorldPosition, transform.forward, out RaycastHit hit, 100f))
                {
                    var blockItem = hit.transform.gameObject.GetComponent<BlockItem>();
                    if (blockItem != null)
                    {
                        var startBlock = AStar.PathFinding(mapCreater.board, mapCreater.GetBlockItem(player.transform.localPosition).block , blockItem.block);
                        player.Move(startBlock);
                    }
                }
            }
        }

        public BlockItem GetBlockItem(Block block)
        {
            var blockItems = mapCreater.GetComponentsInChildren<BlockItem>(true);
            if (blockItems != null)
            {
                return Array.Find(blockItems, item => item.block.x == block.x && item.block.y == block.y);
            }
            return null;
        }
    }
}
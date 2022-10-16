using RaceManager.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RaceManager.Alt
{
    public class AStarLite : MonoBehaviour
    {
        private int _gridSizeX = 1000;
        private int _gridSizeY = 1000;

        private float _cellSize = 2f;

        private AStarNode[,] _aStarNodes;

        private void Start()
        {
            CreateGrid();
        }

        private void CreateGrid()
        {
            _aStarNodes = new AStarNode[_gridSizeX, _gridSizeY];
            for (int x = 0; x < _gridSizeX; x++)
                for (int y = 0; y < _gridSizeY; y++)
                {
                    _aStarNodes[x, y] = new AStarNode(new Vector2Int(x, y));
                    Vector3 worldPos = ConvertGridPosToWorldPos(_aStarNodes[x, y]);
                    Collider[] hitCilliders = Physics.OverlapSphere(worldPos, _cellSize * 0.5f);

                    if (hitCilliders != null)
                    {
                        for (int i = 0; i < hitCilliders.Length; i++)
                        {
                            if (hitCilliders[i].transform.CompareTag(Tag.Obstacle))
                                _aStarNodes[x, y].isObstacle = true;
                        }
                    }
                }
        }

        private Vector3 ConvertGridPosToWorldPos(AStarNode aStarNode)
        {
            float halfX = (_gridSizeX * _cellSize) * 0.5f;
            float halfY = (_gridSizeY * _cellSize) * 0.5f;//z

            return new Vector3(aStarNode.gridPosition.x * _cellSize - halfX, 0f, aStarNode.gridPosition.y * _cellSize - halfY);
        }

        private void OnDrawGizmos()
        {
            if (_aStarNodes == null)
                return;

            for (int x = 0; x < _gridSizeX; x++)
                for (int y = 0; y < _gridSizeY; y++)
                {
                    Gizmos.color = _aStarNodes[x, y].isObstacle ? Color.red : Color.green;
                    Vector3 center = ConvertGridPosToWorldPos(_aStarNodes[x, y]);
                    Vector3 size = new Vector3(_cellSize, _cellSize, _cellSize);
                    Gizmos.DrawWireCube(center, size);
                }
        }
    }
}


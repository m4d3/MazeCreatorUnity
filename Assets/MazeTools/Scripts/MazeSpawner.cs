using System;
using System.Collections;
using UnityEngine;

namespace Assets.MazeTools.Scripts
{
    public class MazeSpawner : MonoBehaviour
    {
        private int _spawnCount;
        private float _spawnDelay;
        public int CurrentWave = -1;

        public WaveInfo[] Data;

        private void Start()
        {
            StartCoroutine("StartWave");
        }


        private IEnumerator Spawn()
        {
            while (_spawnCount > 0)
            {
                Instantiate(Data[CurrentWave].SpawnObject, transform.position, Quaternion.identity);
                _spawnCount--;

                yield return new WaitForSeconds(_spawnDelay);
            }

            StopAllCoroutines();
            StartCoroutine("StartWave");
        }

        private IEnumerator StartWave()
        {
            if (CurrentWave >= Data.Length - 1) yield break;

            CurrentWave++;

            yield return new WaitForSeconds(Data[CurrentWave].WaveDelay);

            _spawnCount = Data[CurrentWave].SpawnCount;
            _spawnDelay = Data[CurrentWave].SpawnDelay;

            StartCoroutine("Spawn");
        }

        [Serializable]
        public struct WaveInfo
        {
            public int SpawnCount;
            public float SpawnDelay;
            public float WaveDelay;
            public GameObject SpawnObject;
        }
    }
}
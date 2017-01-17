using System.Collections;
using UnityEngine;

namespace Assets.MazeTools
{
    public class MazeSpawner : MonoBehaviour
    {
        private int _spawnCount;
        private float _spawnDelay;
        public int currentWave = -1;

        [System.Serializable]
        public struct WaveInfo
        {
            public int SpawnCount;
            public float SpawnDelay;
            public float WaveDelay;
            public GameObject SpawnObject;
        }

        public WaveInfo[] Data;


        // Use this for initialization
        private void Start()
        {
            StartCoroutine("StartWave");
        }
 

        private IEnumerator Spawn()
        {
            while (_spawnCount > 0)
            {
                Debug.Log("Spawning");
                Instantiate(Data[currentWave].SpawnObject, transform.position, Quaternion.identity);
                _spawnCount--;

                yield return new WaitForSeconds(_spawnDelay);
            }

            StopAllCoroutines();
            StartCoroutine("StartWave");
        }

        private IEnumerator StartWave()
        {
            if (currentWave >= Data.Length - 1) yield break;

            currentWave++;

            yield return new WaitForSeconds(Data[currentWave].WaveDelay);

            Debug.Log("new Wave");

            _spawnCount = Data[currentWave].SpawnCount;
            _spawnDelay = Data[currentWave].SpawnDelay;

            StartCoroutine("Spawn");


        }
    }
}
using UnityEngine;
using System.Collections;
using System;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; } //싱글톤 패턴

    [System.Serializable]
    public class EnemyGroup//코드에서 Wave 단위 사용 가능 (변수 세트)
    {
        public GameObject enemyPrefab; //해당 웨이브에서 생성될 적의 프리팹을 담는 변수
        public int count; //적을 몇 마리 생성할지 숫자를 담는 변수
    }

    [System.Serializable]
    public class Wave
    {
        public EnemyGroup[] enemyGroups; //웨이브에 등장하는 적 목록
        public float spawnRate; //1초에 몇 마리씩 생성할지 속도를 담는 변수
    }

    [Header("WaveSetting")] //인스펙터 창에서 보이는 글자. 바꿔도 무방
    public Wave[] waves; //[]배열 Wave 변수 세트를 담는 목록(enemyPrefab, count, spawnRate)
    public Transform[] spawnPoints; //적이 등장할 수 있는 시작 지점들의 목록 (빈 게임 오브젝트들을 원하는 시작위치에 연결)
    public Transform[] waypoints; //이동 경로

    [Header("TimeSetting")]
    [Tooltip("다음 웨이브 호출 가능까지의 대기 시간(초)")]
    public float timeUntilNextWave = 15f;
    [Tooltip("다음 웨이브를 1초 일찍 시작할 때마다 받을 골드 보너스")]
    public int goldBonusPerSecond = 1;

    //이벤트(외부로 상태를 알림)
    public event Action<int> OnNewWave; //새로운 웨이브가 시작됨을 알리는 신호
    public event Action<float> OnCountdownChanged; //카운트다운이 진행되는 동안 계속 발생하는 신호(실시간 시간 정보)

    //상태 변수(내부 상황을 기억)
    private int nextWaveIndex = 0; //웨이브 몇 번재인지 알리는 신호
    private float waveCountdown; //다음웨이브까지 남은 시간을 저장하는 변수
    private bool isCountingDown = false; //카운트다운 진행 여부 
    private Coroutine countdownCoroutine; //다음 버튼 누를 시 코루틴 강제 중지

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);//싱글톤 패턴
        else Instance = this;
    }

    void Start()
    {
        TriggerNextWave(); //게임이 시작되면 즉시 첫 웨이브를 시작
    }

    public void TriggerNextWave() //UI버튼 또는 자동 호출에 의해 다음 웨이브를 시작하는 함수
    {
        if (countdownCoroutine != null) //현재 카운트다운이 실행 되는가 여부(null-실행 안됨 !=null-실행 중)
        {
            StopCoroutine(countdownCoroutine);
        }
        isCountingDown = false; //스위치를 끔
        OnCountdownChanged?.Invoke(0);//화면에 표시되는 시간을 0으로 맞춤

        if (nextWaveIndex >= waves.Length)//다음웨이브 순번이 웨이브의 총 개수보다 크다면 끝
        {
#if UNITY_EDITOR
            Debug.Log("모든 웨이브 클리어!");
#endif
            OnNewWave?.Invoke(0); //UI에 웨이브 종료 신호 (0 전달)
            return;
        }

        //현재 웨이브 스폰 시작
        StartCoroutine(SpawnWave(waves[nextWaveIndex]));
        OnNewWave?.Invoke(nextWaveIndex + 1);

        //다음 웨이브 인덱스 증가
        nextWaveIndex++;

        //아직 다음 웨이브가 남아있다면, 다음 웨이브를 위한 카운트다운 시작
        if (nextWaveIndex < waves.Length)
        {
            countdownCoroutine = StartCoroutine(CountdownForNextWave());
        }
    }
    //웨이브 미리 시작
    public void StartNextWaveEarly()
    {
        if (isCountingDown)
        {
            //보너스 계산 및 지급
            int bonus = (int)waveCountdown * goldBonusPerSecond;
            if (bonus > 0 && ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddGold(bonus);
            }
            //즉시 다음 웨이브 시작
            TriggerNextWave();
        }
    }

    private IEnumerator CountdownForNextWave()
    {
        isCountingDown = true;
        waveCountdown = timeUntilNextWave;
        while (waveCountdown > 0)
        {
            OnCountdownChanged?.Invoke(waveCountdown);
            waveCountdown -= Time.deltaTime;
            yield return null;
        }
        isCountingDown = false;
        TriggerNextWave();
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        foreach (var group in wave.enemyGroups)
        {
            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(1f / wave.spawnRate);
            }
        }
    }

    void SpawnEnemy(GameObject enemyPrefab)
    {
        int spawnIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPoints[spawnIndex].position, Quaternion.identity);

        Enemy enemyScript = enemyObj.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.waypoints = waypoints;
        }
    }
}

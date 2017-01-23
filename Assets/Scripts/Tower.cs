using UnityEngine;

public class Tower : MonoBehaviour
{
    private float _buildingStatus;
    private float _bulletSpeed = 3.0f;
    private float _currentDelay;
    private bool _isBuild;
    private MazePlacer _placerComp;
    private SelectionComponent _selectionComp;
    private readonly float _shootDelay = 4.0f;
    public float BuildingTime;
    public GameObject Bullet;
    public float ShootRadius;

    private void Awake()
    {
        _placerComp = gameObject.AddComponent<MazePlacer>();
        _selectionComp = gameObject.AddComponent<SelectionComponent>();
        _selectionComp.Type = GetType();
        transform.localScale = new Vector3(1, 0.1f, 1);

        _bulletSpeed = Bullet.GetComponent<Bullet>().Speed;
        _currentDelay = _shootDelay;
    }

    private void Update()
    {
        if (_isBuild)
            if (_currentDelay < _shootDelay)
                _currentDelay += Time.deltaTime;
            else
                Shoot();
    }

    private void BuildingFinished()
    {
        _isBuild = true;
        GetComponent<Renderer>().material.color = Color.yellow;
    }

    public bool Build(float buildingSpeed)
    {
        if (_isBuild) return false;

        if (_buildingStatus < BuildingTime)
        {
            _buildingStatus += buildingSpeed * Time.deltaTime;
            transform.localScale = new Vector3(1.0f, _buildingStatus / BuildingTime, 1.0f);
            return true;
        }
        BuildingFinished();
        return false;
    }

    private void Shoot()
    {
        int layerMask = 1 << 8;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ShootRadius, layerMask);
        Enemy closestEnemy = null;
        float distance = ShootRadius;

        foreach (Collider hitCollider in hitColliders)
        {
            if (!hitCollider.GetComponent<Enemy>()) continue;

            float calculatedDistance = Vector3.Distance(hitCollider.transform.position, transform.position);

            if (!(calculatedDistance < distance)) continue;

            distance = calculatedDistance;
            closestEnemy = hitCollider.GetComponent<Enemy>();
        }

        if (closestEnemy == null) return;

        Vector3 enemySpeedVector = closestEnemy.transform.forward * closestEnemy.Speed;
        Vector3 enemyPosition = closestEnemy.transform.position;
        Vector3 target = CalculateInterceptCourse(enemyPosition,
            enemySpeedVector, transform.position, _bulletSpeed);

        if (target == Vector3.zero) return;

        target.Normalize();
        float interceptionTime1 = FindClosestPointOfApproach(enemyPosition, enemySpeedVector, transform.position,
            target * _bulletSpeed);
        Vector3 aimTarget = closestEnemy.transform.position + enemySpeedVector * interceptionTime1;

        Debug.DrawLine(transform.position, aimTarget);
        transform.LookAt(aimTarget);

        GameObject newBullet = Instantiate(Bullet, transform.position, Quaternion.identity);
        newBullet.transform.LookAt(aimTarget);
        _currentDelay = 0;
    }

    private Vector3 CalculateInterceptCourse(Vector3 aTargetPos, Vector3 aTargetSpeed, Vector3 aInterceptorPos,
        float aInterceptorSpeed)
    {
        Vector3 targetDir = aTargetPos - aInterceptorPos;

        float iSpeed2 = aInterceptorSpeed * aInterceptorSpeed;
        float tSpeed2 = aTargetSpeed.sqrMagnitude;
        float fDot1 = Vector3.Dot(targetDir, aTargetSpeed);
        float targetDist2 = targetDir.sqrMagnitude;
        float d = fDot1 * fDot1 - targetDist2 * (tSpeed2 - iSpeed2);
        if (d < 0.1f) // negative == no possible course because the interceptor isn't fast enough
            return Vector3.zero;
        float sqrt = Mathf.Sqrt(d);
        float S1 = (-fDot1 - sqrt) / targetDist2;
        float S2 = (-fDot1 + sqrt) / targetDist2;

        if (S1 < 0.0001f)
            if (S2 < 0.0001f)
                return Vector3.zero;
            else
                return S2 * targetDir + aTargetSpeed;
        if (S2 < 0.0001f)
            return S1 * targetDir + aTargetSpeed;
        if (S1 < S2)
            return S2 * targetDir + aTargetSpeed;
        return S1 * targetDir + aTargetSpeed;
    }

    public static float FindClosestPointOfApproach(Vector3 aPos1, Vector3 aSpeed1, Vector3 aPos2, Vector3 aSpeed2)
    {
        Vector3 PVec = aPos1 - aPos2;
        Vector3 SVec = aSpeed1 - aSpeed2;
        float d = SVec.sqrMagnitude;
        // if d is 0 then the distance between Pos1 and Pos2 is never changing
        // so there is no point of closest approach... return 0
        // 0 means the closest approach is now!
        if (d >= -0.0001f && d <= 0.0002f)
            return 0.0f;
        return -Vector3.Dot(PVec, SVec) / d;
    }
}
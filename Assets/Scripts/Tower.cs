using UnityEngine;

public class Tower : MonoBehaviour
{
    private readonly float _shootDelay = 4.0f;
    private Vector3 _aimTarget;
    private float _buildingStatus;
    private float _bulletSpeed = 3.0f;
    private float _currentDelay;
    private bool _isBuild;
    private MazePlacer _placerComp;
    private SelectionComponent _selectionComp;
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
            if (LookAtNearestEnemy())
                if (_currentDelay < _shootDelay)
                    _currentDelay += Time.deltaTime;
                else
                    Shoot();
            else
                _currentDelay = _shootDelay;
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

    private bool LookAtNearestEnemy()
    {
        var layerMask = 1 << 8;
        var hitColliders = Physics.OverlapSphere(transform.position, ShootRadius, layerMask);
        Enemy closestEnemy = null;
        var distance = ShootRadius;

        foreach (var hitCollider in hitColliders)
        {
            if (!hitCollider.GetComponent<Enemy>()) continue;

            var calculatedDistance = Vector3.Distance(hitCollider.transform.position, transform.position);

            if (!(calculatedDistance < distance)) continue;

            distance = calculatedDistance;
            closestEnemy = hitCollider.GetComponent<Enemy>();
        }

        if (closestEnemy == null) return false;

        var enemySpeedVector = closestEnemy.transform.forward * closestEnemy.Speed;
        var enemyPosition = closestEnemy.transform.position;
        var target = CalculateInterceptCourse(enemyPosition,
            enemySpeedVector, transform.position, _bulletSpeed);

        if (target == Vector3.zero) return false;

        target.Normalize();
        var interceptionTime1 = FindClosestPointOfApproach(enemyPosition, enemySpeedVector, transform.position,
            target * _bulletSpeed);
        _aimTarget = closestEnemy.transform.position + enemySpeedVector * interceptionTime1;

        Debug.DrawLine(transform.position, _aimTarget);
        transform.LookAt(_aimTarget);

        return true;
    }

    private void Shoot()
    {
        var newBullet = Instantiate(Bullet, transform.position, Quaternion.identity);
        newBullet.transform.LookAt(_aimTarget);
        _currentDelay = 0;
    }

    private Vector3 CalculateInterceptCourse(Vector3 aTargetPos, Vector3 aTargetSpeed, Vector3 aInterceptorPos,
        float aInterceptorSpeed)
    {
        var targetDir = aTargetPos - aInterceptorPos;

        var iSpeed2 = aInterceptorSpeed * aInterceptorSpeed;
        var tSpeed2 = aTargetSpeed.sqrMagnitude;
        var fDot1 = Vector3.Dot(targetDir, aTargetSpeed);
        var targetDist2 = targetDir.sqrMagnitude;
        var d = fDot1 * fDot1 - targetDist2 * (tSpeed2 - iSpeed2);
        if (d < 0.1f) // negative == no possible course because the interceptor isn't fast enough
            return Vector3.zero;
        var sqrt = Mathf.Sqrt(d);
        var S1 = (-fDot1 - sqrt) / targetDist2;
        var S2 = (-fDot1 + sqrt) / targetDist2;

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
        var PVec = aPos1 - aPos2;
        var SVec = aSpeed1 - aSpeed2;
        var d = SVec.sqrMagnitude;
        // if d is 0 then the distance between Pos1 and Pos2 is never changing
        // so there is no point of closest approach... return 0
        // 0 means the closest approach is now!
        if (d >= -0.0001f && d <= 0.0002f)
            return 0.0f;
        return -Vector3.Dot(PVec, SVec) / d;
    }
}
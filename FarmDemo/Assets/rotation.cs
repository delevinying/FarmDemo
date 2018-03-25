using UnityEngine;
using System.Collections;

public class rotation : MonoBehaviour
{

    public static bool Kongrotating = true;

    //-----------------  初始参数  -----------------

    //--旋转速度
    private float xSpeed = 125.0f;
    private float ySpeed = 60.0f;

    //--缩放速度
    private float wheelSpeed = 50;

    //--移动速度
    private float xMoveSpeed = 200;
    private float yMoveSpeed = 200;

    //--是否无限制放大，但仍旧会受移动限制所规定的范围
    private bool IsYYFD = true;
    //--镜头与中心点距离，在 IsYYFD 变量为 true 时有效
    public static float JvLi;

    //-----------------  移动限制  -----------------

    //--是否允许移动相机
    public static bool IsCanMove;
    //--是否允许旋转相机
    public static bool IsCanRotate;
    //--是否允许缩放相机
    public static bool IsCanZoom;

    //--是否开启位置限制
    private bool IsCheck;
    //--缩放限制，最大，最小
    private float MaxWheel = 300;
    private float MinWheel = 8;

    //--移动限制

    //x轴最大值、最小值
    private float xMaxMove = 94;
    private float xMinMove = -94;
    //y轴最大值、最小值
    private float yMaxMove = 58;
    private float yMinMove = 1;
    //z轴最大值、最小值
    private float zMaxMove = 44;
    private float zMinMove = -44;

    //中心点物体
    public static Transform Tanks;
    //缩放控制
    private float distance;
    //位置控制
    private Vector3 position;
    //角度控制
    private Quaternion rotation1;
    //相机动画控制：0停止；1旋转指定角度；2旋转到某个角度
    static int Ani_Camera;
    //旋转动画的角度
    static float XAniAngle = 0f;
    static float YAniAngle = 0f;
    static float ZAniAngle = 0f;
    //旋转动画的方向
    static bool XFX;
    static bool YFX;
    static bool ZFX;
    //是否强行跳转
    static bool IsQXTZ = false;
    //是否跟随事件移动
    public static bool IsFollow = false;
    //锁定物体名称
    static string LockModName = "";
    //旋转速度
    static float XZSD = 1;
    //-----------------  无需改动，中间变量  -----------------

    //旋转角度（二维屏幕）
    public static float x = 90.0f;
    public static float y = 30.0f;
    public static float z = 0.0f;
    //坐标还原点
    private Vector3 LPosition;
    //中心物体坐标还原点
    private Vector3 LTPosition;

    //-----------------  公用函数  -----------------

    //使相机跳转到模型
    public static bool MoveToMod(string ModName)
    {
        if (ModName == null || ModName == "") return false;
        //获得模型名称的GameObject
        Transform newTran = GameObject.Find(ModName).transform;
        //如果获取到了
        if (newTran != null)
        {
            //设置相机位置
            Tanks.position = newTran.position;
            IsQXTZ = true;
            //返回成功
            return true;
        }
        //如果失败
        else
        {
            //返回假
            return false;
        }
    }
    //使相机跳转到模型并按一定角度跳转
    public static void CameraRotate(string ModName, float XAngle, float YAngle, float ZAngle, float Velocity, bool LsFollow)
    {
        //设置是否跟随视角
        IsFollow = LsFollow;
        //设置相机位置
        LockModName = ModName;
        //设置旋转角度
        XAniAngle = XAngle;
        YAniAngle = YAngle;
        ZAniAngle = ZAngle;
        //设置速度
        XZSD = Velocity;
        //初始化相机动画控制
        Ani_Camera = 1;
    }
    //相机旋转到某个角度
    public static void CameraRotateTo(string ModName, float XAngle, float YAngle, float ZAngle, float Velocity, bool LsFollow)
    {
        //throw new Exception(ModName);
        //设置是否跟随视角
        IsFollow = LsFollow;
        //设置相机位置
        LockModName = ModName;
        //设置旋转角度
        XAniAngle = XAngle;
        YAniAngle = YAngle;
        ZAniAngle = ZAngle;
        //设置速度
        XZSD = Velocity;
        //定义偏移速度
        XFX = (XAniAngle > x) ? true : false;
        YFX = (YAniAngle > y) ? true : false;
        ZFX = (ZAniAngle > z) ? true : false;
        //开启动画控制
        Ani_Camera = 2;

    }
    //停止旋转
    public static void StopXZ()
    {
        Ani_Camera = 0;
    }
    //格式化X、Y、Z轴
    public static void FormatXYZ()
    {
        y = y % 360;
        x = x % 360;
        z = z % 360;
        //y = (360+y) % 360;
        //x = (360+x) % 360;
        //z = (360+z) % 360;
        //if (y < 0) y = 360 + y;
        //if (x < 0) x = 360 + x;
        //if (z < 0) z = 360 + z;
    }

    //-----------------  系统函数  -----------------
    void Start()
    {
        //初始化中心模块
        Tanks = GameObject.Find("Main Camera").transform;
        //初始化相机动画控制器
        Ani_Camera = 0;
        //初始化相机控制权，默认全部允许
        IsCanMove = true;
        IsCanRotate = true;
        IsCanZoom = true;
        //开启移动范围距离
        IsCheck = true;
        //关闭相机视角锁定
        IsFollow = false;
        //初始化相机位置
        transform.position = new Vector3(-20.8f, 33.8f, 36.9f);
        //初始化相机与中心模型的距离
        JvLi = 30;
        //初始化相机角度
        x = 180; y = 45; z = 0;
        //初始化中心物体位置
        Tanks.position = new Vector3(-19.6f, 6.7f, 5.2f);
        //初始化中心物体角度
        Tanks.rotation = Quaternion.Euler(0, 0, 0);
        //强制旋转
        IsQXTZ = true;
    }

    void Update()
    {
        if (!Kongrotating) return;
        //如果没有得到中心模块则此脚本无效
        if (Tanks == null) return;
        //初始坐标还原点
        LPosition = transform.position;
        //初始中心物体坐标还原点
        LTPosition = Tanks.position;
        if (IsFollow) MoveToMod(LockModName);

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            x -= 2;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            x += 2;
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            y--;
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            y++;
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * 30 * Time.deltaTime);
            Tanks.Translate(Vector3.forward * 30 * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.forward * -30 * Time.deltaTime);
            Tanks.Translate(Vector3.forward * -30 * Time.deltaTime);
        }

        //旋转指定角度相机动画
        if (Ani_Camera == 1)
        {

            //当还有剩余旋转角度则继续旋转
            if (XAniAngle > 0)
            {
                //横向坐标加旋转速度
                x += XZSD;
                //剩余角度减去速度
                XAniAngle -= XZSD;
            }
            if (YAniAngle > 0)
            {
                //横向坐标加旋转速度
                y += XZSD;
                //剩余角度减去速度
                YAniAngle -= XZSD;
            }
            //如果没有剩余旋转角度
            if (YAniAngle <= 0 && XAniAngle <= 0)
            {
                //设置强制跳转无效
                IsQXTZ = false;
                //关闭动画控制器
                Ani_Camera = 0;
                //取消跟踪
                IsFollow = false;
            }
        }
        //旋转到角度动画
        else if (Ani_Camera == 2)
        {
            //检查是否完成
            int IsCom = 0;
            if (!XFX && XAniAngle < x - XZSD)
            {
                x -= XZSD;
            }
            else if (XFX && XAniAngle > x + XZSD)
            {
                x += XZSD;
            }
            else if ((!XFX && XAniAngle >= x - XZSD) || (XFX && XAniAngle <= x + XZSD))
            {
                IsCom++;
            }
            if (!YFX && YAniAngle < y - XZSD)
            {
                y -= XZSD;
            }
            else if (YFX && YAniAngle > y + XZSD)
            {
                y += XZSD;
            }
            else if ((!YFX && YAniAngle >= y - XZSD) || (YFX && YAniAngle <= y + XZSD))
            {
                IsCom++;
            }
            if (!ZFX && ZAniAngle < z - XZSD)
            {
                z -= XZSD;
            }
            else if (ZFX && ZAniAngle > z + XZSD)
            {
                z += XZSD;
            }
            else if ((!ZFX && ZAniAngle >= z - XZSD) || (ZFX && ZAniAngle <= z + XZSD))
            {
                IsCom++;
            }
            IsQXTZ = true;
            if (IsCom >= 3)
            {
                //设置强制跳转无效
                //IsQXTZ = false;
                //关闭动画控制器
                Ani_Camera = 0;
                //关闭锁定模型
                IsFollow = false;
            }

        }
        //获取相机与模型的距离
        distance = Vector3.Distance(transform.position, Tanks.position);
        //检查相机是否是以无效放大的模式
        if (IsYYFD)
        {
            //是则缩放时方快跟随移动
            transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * wheelSpeed);
            Tanks.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * wheelSpeed);

            //方块与相机保持相应距离
            distance = JvLi;
        }
        else
        {
            //限制缩放
            if ((Input.GetAxis("Mouse ScrollWheel") < 0 || distance > MinWheel) && (Input.GetAxis("Mouse ScrollWheel") > 0 || distance < MaxWheel) || !IsCheck || IsQXTZ)
                //如果允许缩放
                if (IsCanZoom)
                    //缩放相机
                    distance -= Input.GetAxis("Mouse ScrollWheel") * wheelSpeed;
        }

        //按下鼠标右键执行旋转
        if (Input.GetMouseButton(1) && IsCanRotate)
        {
            //设置横向旋转距离
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            //设置纵向旋转距离
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }
        //格式化 x y z 坐标，使之不可超越-360~360
        FormatXYZ();
        //设置角度
        rotation1 = Quaternion.Euler(y, x, z);
        //设置位置
        position = rotation1 * new Vector3(0.0f, 0.0f, -distance) + Tanks.position;
        //更新角度
        Tanks.rotation = rotation1;
        //更新位置
        if (Input.GetMouseButton(1) && IsCanRotate || IsQXTZ)
        {
            transform.position = position;
            IsQXTZ = false;
        }
        else
        {
            transform.position = rotation1 * new Vector3(0.0f, 0.0f, 0.0f) + transform.position;
        }
        Tanks.position = rotation1 * new Vector3(0.0f, 0.0f, 0.0f) + Tanks.position;
        //鼠标中间平移
        if (Input.GetMouseButton(2) && IsCanMove)
        {
            //横向平移
            transform.Translate(Vector3.right * xMoveSpeed * Time.deltaTime * -Input.GetAxis("Mouse X"));
            //纵向平移，若相机垂直地面则向前平移
            transform.Translate(Vector3.up * yMoveSpeed * Time.deltaTime * -Input.GetAxis("Mouse Y"));

            //横向平移
            Tanks.Translate(Vector3.right * xMoveSpeed * Time.deltaTime * -Input.GetAxis("Mouse X"));
            //纵向平移，若相机垂直地面则向前平移
            Tanks.Translate(Vector3.up * yMoveSpeed * Time.deltaTime * -Input.GetAxis("Mouse Y"));
        }
        //检查相机x轴
        if ((transform.position.x > xMaxMove || transform.position.x < xMinMove) && IsCheck)
        {
            transform.position = new Vector3(LPosition.x, transform.position.y, transform.position.z);
        }
        //检查相机y轴
        if ((transform.position.y > yMaxMove || transform.position.y < yMinMove) && IsCheck)
        {
            transform.position = new Vector3(transform.position.x, LPosition.y, transform.position.z);
        }
        //检查相机z轴
        if ((transform.position.z > zMaxMove || transform.position.z < zMinMove) && IsCheck)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, LPosition.z);
        }

        //检查中心物体x轴
        if ((Tanks.position.x > xMaxMove || Tanks.position.x < xMinMove) && IsCheck)
        {
            Tanks.position = new Vector3(LTPosition.x, Tanks.position.y, Tanks.position.z);
        }
        //检查中心物体y轴
        if ((Tanks.position.y > yMaxMove || Tanks.position.y < yMinMove) && IsCheck)
        {
            Tanks.position = new Vector3(Tanks.position.x, LTPosition.y, Tanks.position.z);
        }
        //检查中心物体z轴
        if ((Tanks.position.z > zMaxMove || Tanks.position.z < zMinMove) && IsCheck)
        {
            Tanks.position = new Vector3(Tanks.position.x, Tanks.position.y, LTPosition.z);
        }

        //更新角度
        transform.rotation = Tanks.rotation;
    }

}
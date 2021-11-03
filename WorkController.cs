using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;


namespace Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        public WorkController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public ActionResult Get()
        {
            return Json(DAL.WorkInfo.Instance.GetCount());
        }

        [HttpGet("new")]
        public ActionResult GetNew()  //获取推荐活动
        {
            var result = DAL.WorkInfo.Instance.GetNew();
            if (result.Count() != 0)
                return Json(Result.Ok(result));
            else
                return Json(Result.Err("记录数为0"));
        }
        [HttpGet("{id}")]
        public ActionResult Get(int id) //获取指定id活动，并将活动说明的图片src添加网站路径，使客户端能访问图片
        {
            var result = DAL.WorkInfo.Instance.GetModel(id);
            if (result != null)
                return Json(Result.Ok(result));
            else
                return Json(Result.Err("WorkId不存在"));
        }
        [HttpPost]
        public ActionResult Post([FromBody] Model.WorkInfo workInfo)  //发布活动(添加新活动)，并将src的网站路径删除，防止服务器地址变化时，客户端无法访问
        {
            workInfo.recommend = "否";
            workInfo.workVerify = "待审核";
            workInfo.uploadTime = DateTime.Now;
            try
            {
                int n = DAL.WorkInfo.Instance.Add(workInfo);
                return Json(Result.Ok("发布活动成功", n));
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("foreign key"))
                    if (ex.Message.ToLower().Contains("username"))
                        return Json(Result.Err("合法用户才能添加记录"));
                    else
                        return Json(Result.Err("作品所属活动不存在"));
                else if (ex.Message.ToLower().Contains("null"))
                    return Json(Result.Err("活动名称、结束时间、活动图片、活动审核情况、用户名不能为空"));
                else
                    return Json(Result.Err(ex.Message));
            }
        }
        [HttpPut]
        public ActionResult Put([FromBody] Model.WorkInfo workInfo ) //修改活动
        {
            workInfo.recommend = "否";
            workInfo.workVerify = "待审核";
            workInfo.uploadTime = DateTime.Now;
            try
            {
                var n = DAL.WorkInfo.Instance.Update(workInfo);
                if (n != 0)
                    return Json(Result.Ok("修改作品成功", workInfo.activityId));
                else
                    return Json(Result.Err("workId不存在"));
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("null"))
                    return Json(Result.Err("活动名称、结束时间、活动图片、活动审核情况不能为空"));
                else
                    return Json(Result.Err(ex.Message));
            }
        }
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)  
        {
            try
            {
                var n = DAL.WorkInfo.Instance.Delete(id);
                if (n != 0)
                    return Json(Result.Ok("删除成功"));
                else
                    return Json(Result.Err("workId不存在"));

            }
            catch (Exception ex)
            {
                return Json(Result.Err(ex.Message));
            }
        }
        [HttpGet("count")]
        public ActionResult getCount([FromBody]int[]activityIds)
        {
            return Json(DAL.WorkInfo.Instance.GetCount(activityIds));
        }
        [HttpPost("page")]  
        public ActionResult getPage([FromBody] Model.Page page)
        {
            var result = DAL.WorkInfo.Instance.GetPage(page);
            if (result.Count() == 0)
                return Json(Result.Err("返回记录数为0"));
            else
                return Json(Result.Ok(result));
        }
        [HttpGet("findCount")]
        public ActionResult getFindCount(string findName)
        {
            if (userName == null) userName = "";
            return Json(DAL.WorkInfo.Instance.GetCount(findName));
        }
        [HttpPost("findPage")] //分页获取审核通过活动数据
        public ActionResult getFindPage([FromBody] Model.WorkFindPage page)
        {
            if (page.userName == null) page.userName = "";
            var result = DAL.WorkInfo.Instance.GetFindPage(page);
            if (result.Count() == 0)
                return Json(Result.Err("返回记录数为0"));
            else
                return Json(Result.Ok(result));
        }
        [HttpPost("myPage")]
        public ActionResult getMyPage([FromBody] Model.WorkFindPage page)
        {
            if (page.userName == null) page.userName = "";
            var result = DAL.WorkInfo.Instance.GetMyPage(page);
            if (result.Count() == 0)
                return Json(Result.Err("返回记录数为0"));
            else
                return Json(Result.Ok(result));
        }
        [HttpPut("Verify")]
        public ActionResult PutVerify([FromBody]Model.WorkInfo workInfo)  //修改活动审核情况
        {
            try
            {
                var n = DAL.WorkInfo.Instance.UpdateVerify(WorkInfo);
                if (n != 0)
                    return Json(Result.Ok("审核活动成功", workInfo.workId));
                else
                    return Json(Result.Err("workId不存在"));
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("null"))
                    return Json(Result.Err("活动审核情况不能为空"));
                else
                    return Json(Result.Err(ex.Message));
            }
        }

        [HttpPut("Recommend")]
        public ActionResult PutRecommend([FromBody]Model.WorkInfo workInfo)   //修改活动推荐情况
        {
            WorkInfo.recommendTime = DateTime.Now;
            try
            {
                var re = "";
                if (workInfo.recommend == "否") re = "取消";
                var n = DAL.WorkInfo.Instance.UpdateRecommend(workInfo);

                if (n != 0)
                    return Json(Result.Ok($"{re}推荐活动成功", workInfo.workId));
                else
                    return Json(Result.Err("workId不存在"));
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("null"))
                    return Json(Result.Err("推荐活动情况不能为空"));
                else
                    return Json(Result.Err(ex.Message));
            }
        }
        [HttpPut("{id}")]
        public ActionResult upImg(int id, List<IFormFile> files)
        {

            var path = System.IO.Path.Combine(_hostingEnvironment.WebRootPath, "img", "Work")
            var fileName = $"{path}/{id}";
            try
            {
                var ext = DAL.Upload.Instance.UpImg(files[0], fileName);
                if (ext == null)
                    return Json(Result.Err("请上传图片文件"));
                else
                {
                    var file = $"img/Work/{id}{ext}";
                    Model.WorkInfo workInfo = new Model.WorkInfo();
                    if (id.StartsWith("i"))
                    {
                        workInfo.workId = int.Parse(id.Substring(1));
                        workInfo.workIntroduction = file;
                      }
                    else
                    {
                        workInfo.workId = int.Parse(id);
                        workInfo.workPicture = file;
                    }
                    var n = DAL.WorkInfo.Instance.UpdateImg(workInfo);
                    if (n > 0)
                        return Json(Result.Ok("上传成功", file));
                    else
                        return Json(Result.Err("请输入正确的活动id"));
                }
            }
            catch (Exception ex)
            {
                return Json(Result.Err(ex.Message));
            }
        }
    }
}

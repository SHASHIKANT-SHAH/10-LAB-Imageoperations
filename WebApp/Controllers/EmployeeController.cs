using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using System.IO;
using WebApp.Interfaces;

namespace WebApp.Controllers
{
    public class EmployeeController : Controller
    {
        AppDbContext _db;
        private  IWebHostEnvironment webHostEnvironment;
        IFileHelper _file;

        public EmployeeController(AppDbContext db, IWebHostEnvironment webHostEnvironment, IFileHelper file)
        {
            _db = db;
            this.webHostEnvironment = webHostEnvironment;
            _file = file;
        }

        public IActionResult Index()
        {
            List<EmployeeViewModel> EmpDeptViewModellist = new List<EmployeeViewModel>();
            var empList = (from e in _db.Employees
                           select new
                           {
                               e.EmployeeId,
                               e.Name,
                               e.Address,
                               e.ImagePath
                           }).ToList();
            foreach(var emp in empList)
            {
                EmployeeViewModel objedvm = new EmployeeViewModel();
                objedvm.EmployeeId = emp.EmployeeId;
                objedvm.Name = emp.Name;
                objedvm.Address = emp.Address;
                objedvm.ImagePath = emp.ImagePath;
                EmpDeptViewModellist.Add(objedvm);
            }
            return View(EmpDeptViewModellist);
        }

        public IActionResult Create()
        {
            return View();
        }
        private string UploadedFile(EmployeeViewModel model)
        {
            string filePath = null;
            if (model.Image != null)
            {
                string uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "images");
                filePath = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                string path = Path.Combine(uploadsFolder, filePath);
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    model.Image.CopyTo(fileStream);
                }
            }
            return filePath;
        }
   

        [HttpPost]
        public IActionResult Create(EmployeeViewModel model) 
        {
            ModelState.Remove("EmployeeId");
            if(ModelState.IsValid)
            {
                if (ModelState.IsValid)
                {
                    string filePath = _file.UploadFile(model.Image);
                    Employee employee = new Employee
                    {
                        Name = model.Name,
                        Address = model.Address,
                        ImagePath = filePath

                    };
                    _db.Employees.Add(employee);
                    _db.SaveChanges();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View();
        }
        //public async Task<IActionResult> GetDefaultImage(int id)
        //{
        //    Employee data = _db.Employees.Find(id);
        //    var defaultFilePath = Path.Combine(webHostEnvironment.WebRootPath, "images", data.ImagePath);

        //    byte[] defaultFileBytes;
        //    using (var stream = new FileStream(defaultFilePath, FileMode.Open, FileAccess.Read))
        //    {
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            await stream.CopyToAsync(memoryStream);
        //            defaultFileBytes = memoryStream.ToArray();
        //        }
        //    }

        //    return File(defaultFileBytes, "image/*");
        //}

       

        public  IActionResult Edit(int id) 
        {
            Employee data = _db.Employees.Find(id);
            EmployeeViewModel model = new EmployeeViewModel();
            if (data != null )
            {
                model.Name = data.Name;
                model.Address = data.Address;   
                model.EmployeeId= data.EmployeeId;
                model.ImagePath = data.ImagePath;
            }
            return View("Create",model);
        }

        [HttpPost]
        public IActionResult Edit(EmployeeViewModel model)
        {
           
            
            if(ModelState.IsValid)
            {
                Employee emp = new Employee
                {
                    EmployeeId  = model.EmployeeId,
                    Name = model.Name,
                    Address = model.Address,
                };
                if(model.Image != null)
                {
                    string filepath = UploadedFile(model);
                    emp.ImagePath = filepath;
                }
                _db.Employees.Update(emp);
                _db.SaveChanges();
                return RedirectToAction("Index"); 
            }
            return View("Create",model);
        }

        public IActionResult Delete(int id, string image)
        {
            Employee emp = _db.Employees.Find(id);
            if(emp != null)
            {
                _file.DeleteFile(image);
                _db.Employees.Remove(emp);
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

       
    }
}

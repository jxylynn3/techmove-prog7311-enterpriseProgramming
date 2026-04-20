namespace ST10448420_TechMove_GLMS.UtilsServices
{
    public class PDFManagementService
    {
        public async Task<string> SaveFileAsync(IFormFile file)
        {
            if (file == null || !file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid file type. Only PDFs are allowed.");

            // Ensure directory exists
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "contracts");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var fullPath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the relative path for the database/browser
            return "/uploads/contracts/" + fileName;
        }
    }
}


### **📂 Carpeta `BD/`**  
Contiene recursos relacionados con la base de datos del proyecto:  

- **📄 Scripts SQL**:  
  - Estructura de tablas (`schema.sql`).  
  - Datos iniciales para pruebas (`datos_ejemplo.sql`).  
  - Migraciones o procedimientos almacenados (si aplica).  

- **🎯 Propósito**:  
  - Sirve como **referencia** para replicar la base de datos en otros entornos.  
  - Los archivos pueden adaptarse según necesidades específicas (ej: cambiar nombres de tablas, añadir datasets personalizados).  

- **⚠️ Nota**:  
  - Los datos incluidos son **ejemplificativos** (modifícalos según requerimientos reales).  
  - Si el proyecto usa un ORM (como Entity Framework), consulta también la carpeta 

---

#### **⚡ ¿Cómo usar estos scripts?**  
1. Ejecuta los archivos en orden:  
   ```sql
   -- 1. Primero el esquema
   \i BD/schema.sql  
   -- 2. Luego los datos  
   \i BD/datos_ejemplo.sql  
   ```  
2. Personaliza los archivos antes de desplegar en producción.  


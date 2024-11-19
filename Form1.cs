using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace Reinos_Cristianos
{
    public partial class Form1 : Form
    {
        private string connectionString;
        public Form1()
        {
            InitializeComponent();
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Historia_Medieval.db");

            connectionString = $"Data Source={dbPath};Version=3;";
            LoadComboBoxPersonajes();
            //--------------------------------
            LoadCombosPersonajesParentesco();
            LoadTiposParentescoComboBox();
           // LoadParientesComboBox();
            LoadTipoRelacionComboBox();
            LoadRelativosComboBox();
            LoadComboBoxCargos();

            comboBoxPersonajes.SelectedIndexChanged += ComboBoxPersonajes_selectedIndexChanged; // Vinculación del evento
            comboBoxParentescoPersonaje1.SelectedIndexChanged += comboBoxParentescoPersonaje1_SelectedIndexChanged; // Vinculación del evento
            comboBoxTipoParentesco.SelectedIndexChanged += comboBoxTipoRelaccionEsposos_SelectedIndexChanged; // Vinculación del evento
            comboBoxCargos.SelectedIndexChanged += ComboBoxCargos_SelectedIndexChanged; // Vinculación del evento
            buttonGuardaPersonaje.Click += buttonGuardaPersonaje_click; // Vinculación del evento
            buttonGuardarParentesco.Click += buttonGuardarParentesco_Click; // Vinculación del evento
            buttonGuardarCargo.Click += ButtonGuardarCargo_Click; // Vinculación del evento
        }

        private void LoadComboBoxPersonajes() // puebla el PersonasComboBox con los nombres y apellidos de los personajes
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT nombre, apellido FROM personajes";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string fullName = $"{reader["nombre"]}, {reader["apellido"]}";
                                comboBoxPersonajes.Items.Add(fullName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void ComboBoxPersonajes_selectedIndexChanged(object sender, EventArgs e)
        {
            string selectedFullName = comboBoxPersonajes.SelectedItem.ToString();
            string[] nameParts = selectedFullName.Split(new string[] { ", " }, StringSplitOptions.None);
            string selectedName = nameParts[0];
            string selectedSurname = nameParts[1];
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT nombre, apellido, mote, fecha_nacimiento, fecha_muerte, importancia, biografia FROM personajes WHERE nombre = @Nombre AND apellido = @Apellido";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Nombre", selectedName);
                        command.Parameters.AddWithValue("@Apellido", selectedSurname);
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                textBoxNombrePer.Text = reader["nombre"].ToString();
                                textBoxApellidoPer.Text = reader["apellido"].ToString();
                                textBoxMotePer.Text = reader["mote"].ToString();
                                textBoxFecha_nacimiento.Text = reader["fecha_nacimiento"].ToString();
                                textBoxFecha_muerte.Text = reader["fecha_muerte"].ToString();
                                textBoxImportancia.Text = reader["importancia"].ToString();
                                richTextBoxBiografia.Text = reader["biografia"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void buttonGuardaPersonaje_click(object sender, EventArgs e)
        {
            string nombre = textBoxNombrePer.Text.Trim();
            string apellido = textBoxApellidoPer.Text.Trim();

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido))
            {
                MessageBox.Show("Por favor, ingrese un nombre y apellido.");
                return;
            }

            int personajeId;
            if (ExistePersonaje(nombre, apellido, out personajeId))
            {
                // Realizar UPDATE si el personaje ya existe
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = "UPDATE personajes SET nombre = @Nombre, apellido = @Apellido, mote = @mote, fecha_nacimiento = @fecha_nacimiento, fecha_muerte = @fecha_muerte, importancia = @importancia, biografia = @biografia WHERE id = @Id";
                        using (SQLiteCommand command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Nombre", nombre);
                            command.Parameters.AddWithValue("@Apellido", apellido);
                            command.Parameters.AddWithValue("@Id", personajeId);
                            command.Parameters.AddWithValue("@mote", textBoxMotePer.Text);
                            command.Parameters.AddWithValue("@fecha_nacimiento", textBoxFecha_nacimiento.Text);
                            command.Parameters.AddWithValue("@fecha_muerte", textBoxFecha_muerte.Text);
                            command.Parameters.AddWithValue("@importancia", textBoxImportancia.Text);
                            command.Parameters.AddWithValue("@biografia", richTextBoxBiografia.Text);

                            command.ExecuteNonQuery();
                            MessageBox.Show("Personaje actualizado exitosamente.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
            else
            {
                // Realizar INSERT si el personaje no existe
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = "INSERT INTO personajes (nombre, apellido) VALUES (@Nombre, @Apellido)";
                        using (SQLiteCommand command = new SQLiteCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Nombre", nombre);
                            command.Parameters.AddWithValue("@Apellido", apellido);
                            command.ExecuteNonQuery();
                            MessageBox.Show("Personaje agregado exitosamente.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private bool ExistePersonaje(string nombre, string apellido, out int personajeId)
        {
            personajeId = -1;
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT id FROM personajes WHERE nombre = @Nombre AND apellido = @Apellido";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Nombre", nombre);
                        command.Parameters.AddWithValue("@Apellido", apellido);
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                personajeId = Convert.ToInt32(reader["id"]);
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            return false;
        }
       // -------------------------------
        private void LoadCombosPersonajesParentesco() // puebla los comboBoxPersonaje1 y comboBoxPersonaje2 con los nombres y apellidos de los personajes
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT id, nombre, apellido FROM personajes";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string fullName = $"{reader["id"]}: {reader["nombre"]} {reader["apellido"]}";
                                comboBoxParentescoPersonaje1.Items.Add(fullName);
                                comboBoxParentescoPersonaje2.Items.Add(fullName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }


        private bool ValidarFechas(out DateTime? fechaInicio, out DateTime? fechaFin)
        {
            fechaInicio = null;
            fechaFin = null;

            if (comboBoxTipoParentesco.SelectedItem.ToString().Contains("esposos"))
            {
                if (!DateTime.TryParse(textBoxInicioRelacion.Text, out DateTime tempFechaInicio))
                {
                    MessageBox.Show("Fecha de inicio no válida.");
                    return false;
                }
                fechaInicio = tempFechaInicio;

                if (!DateTime.TryParse(textBoxFinRelacion.Text, out DateTime tempFechaFin))
                {
                    MessageBox.Show("Fecha de fin no válida.");
                    return false;
                }
                fechaFin = tempFechaFin;

                if (fechaInicio.Value.Year < 500 || fechaFin.Value.Year < 500)
                {
                    MessageBox.Show("Las fechas deben ser a partir del siglo VI.");
                    return false;
                }
            }

            return true;
        }

        private void ButtonGuardaRelacionSP_Click(object sender, EventArgs e)
        {
            if (!ValidarFechas(out DateTime? fechaInicio, out DateTime? fechaFin))
            {
                return;
            }

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO relaciones_sociopoliticas (personaje_id1, personaje_id2, tipo_relacion_id, fecha_inicio, fecha_fin) VALUES (@personaje_id1, @personaje_id2, @tipo_relacion_id, @fecha_inicio, @fecha_fin)";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        int personajeId1 = int.Parse(comboBoxRelacionado1.SelectedItem.ToString().Split(':')[0]);
                        int personajeId2 = int.Parse(comboBoxRelacionado2.SelectedItem.ToString().Split(':')[0]);
                        int tipoRelacionId = int.Parse(comboBoxTipoRelacion.SelectedItem.ToString().Split(':')[0]);

                        command.Parameters.AddWithValue("@personaje_id1", personajeId1);
                        command.Parameters.AddWithValue("@personaje_id2", personajeId2);
                        command.Parameters.AddWithValue("@tipo_relacion_id", tipoRelacionId);

                        if (fechaInicio.HasValue)
                        {
                            command.Parameters.AddWithValue("@fecha_inicio", fechaInicio.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@fecha_inicio", DBNull.Value);
                        }

                        if (fechaFin.HasValue)
                        {
                            command.Parameters.AddWithValue("@fecha_fin", fechaFin.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@fecha_fin", DBNull.Value);
                        }

                        command.ExecuteNonQuery();
                    }

                    // Consulta para obtener la relación recíproca
                    string queryReciproca = "SELECT reciproca FROM tipos_relaciones_familiares WHERE id = @tipo_relacion_id";
                    using (SQLiteCommand commandReciproca = new SQLiteCommand(queryReciproca, connection))
                    {
                        int personajeId1 = int.Parse(comboBoxRelacionado1.SelectedItem.ToString().Split(':')[0]);
                        int personajeId2 = int.Parse(comboBoxRelacionado2.SelectedItem.ToString().Split(':')[0]);
                        int tipoRelacionId = int.Parse(comboBoxTipoRelacion.SelectedItem.ToString().Split(':')[0]);

                        commandReciproca.Parameters.AddWithValue("@tipo_relacion_id", tipoRelacionId);
                        object result = commandReciproca.ExecuteScalar();

                        if (result != null)
                        {
                            string resultString = result.ToString();
                            MessageBox.Show($"Valor de result: {resultString}");

                            // Realizar una consulta adicional para obtener el ID de la relación recíproca
                            string queryReciprocaId = "SELECT id FROM tipos_relaciones_familiares WHERE nombre = @nombre";
                            using (SQLiteCommand commandReciprocaId = new SQLiteCommand(queryReciprocaId, connection))
                            {
                                commandReciprocaId.Parameters.AddWithValue("@nombre", resultString);
                                object reciprocaIdResult = commandReciprocaId.ExecuteScalar();

                                if (reciprocaIdResult != null && int.TryParse(reciprocaIdResult.ToString(), out int tipoRelacionReciproca))
                                {
                                    // Insertar el parentesco recíproco
                                    string queryReciprocaInsert = "INSERT INTO parentescos (personaje_id1, personaje_id2, tipo_relacion_id, fecha_inicio, fecha_fin) VALUES (@personaje_id1, @personaje_id2, @tipo_relacion_id, @fecha_inicio, @fecha_fin)";
                                    using (SQLiteCommand commandReciprocaInsert = new SQLiteCommand(queryReciprocaInsert, connection))
                                    {
                                        commandReciprocaInsert.Parameters.AddWithValue("@personaje_id1", personajeId2);
                                        commandReciprocaInsert.Parameters.AddWithValue("@personaje_id2", personajeId1);
                                        commandReciprocaInsert.Parameters.AddWithValue("@tipo_relacion_id", tipoRelacionReciproca);

                                        if (fechaInicio.HasValue)
                                        {
                                            commandReciprocaInsert.Parameters.AddWithValue("@fecha_inicio", fechaInicio.Value);
                                        }
                                        else
                                        {
                                            commandReciprocaInsert.Parameters.AddWithValue("@fecha_inicio", DBNull.Value);
                                        }

                                        if (fechaFin.HasValue)
                                        {
                                            commandReciprocaInsert.Parameters.AddWithValue("@fecha_fin", fechaFin.Value);
                                        }
                                        else
                                        {
                                            commandReciprocaInsert.Parameters.AddWithValue("@fecha_fin", DBNull.Value);
                                        }

                                        commandReciprocaInsert.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("No se encontró un ID de relación recíproca válido.");
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("No se encontró una relación recíproca válida.");
                        }
                    }

                    MessageBox.Show("Relación y relación recíproca agregadas exitosamente.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        //------------------------------- cargos

        private void LoadComboBoxCargoPersonaje()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT id, nombre FROM cargos";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string cargo = $"{reader["id"]}: {reader["nombre"]}";
                                comboBoxCargoPersonaje.Items.Add(cargo);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void LoadComboBoxCargos()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT nombre FROM cargos";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                comboBoxCargos.Items.Add(reader["nombre"].ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void ComboBoxCargos_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedCargo = comboBoxCargos.SelectedItem.ToString();
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT descripcion FROM cargos WHERE nombre = @Nombre";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Nombre", selectedCargo);
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                textBoxCargo.Text = selectedCargo;
                                textBoxCargoDescripcion.Text = reader["descripcion"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void ButtonGuardarCargo_Click(object sender, EventArgs e)
        {
            string nombreCargo = textBoxCargo.Text.Trim();
            string descripcionCargo = textBoxCargoDescripcion.Text.Trim();

            if (string.IsNullOrEmpty(nombreCargo))
            {
                MessageBox.Show("Por favor, ingrese el nombre del cargo.");
                return;
            }

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM cargos WHERE nombre = @Nombre";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Nombre", nombreCargo);
                        int count = Convert.ToInt32(command.ExecuteScalar());

                        if (count > 0)
                        {
                            // Realizar UPDATE si el cargo ya existe
                            query = "UPDATE cargos SET descripcion = @Descripcion WHERE nombre = @Nombre";
                        }
                        else
                        {
                            // Realizar INSERT si el cargo no existe
                            query = "INSERT INTO cargos (nombre, descripcion) VALUES (@Nombre, @Descripcion)";
                        }

                        using (SQLiteCommand commandSave = new SQLiteCommand(query, connection))
                        {
                            commandSave.Parameters.AddWithValue("@Nombre", nombreCargo);
                            commandSave.Parameters.AddWithValue("@Descripcion", descripcionCargo);
                            commandSave.ExecuteNonQuery();
                        }

                        MessageBox.Show("Cargo guardado exitosamente.");
                        LoadComboBoxCargos(); // Recargar el comboBoxCargos para reflejar los cambios
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

//-------------------------------  parientes
        private void LoadParientesComboBox() // puebla los comboBoxPersonaje1 y comboBoxPersonaje2 con los nombres y apellidos de los personajes
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT id, nombre, apellido FROM personajes";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string fullName = $"{reader["id"]}: {reader["nombre"]} {reader["apellido"]}";
                                comboBoxParentescoPersonaje1.Items.Add(fullName);
                                comboBoxParentescoPersonaje2.Items.Add(fullName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void LoadTiposParentescoComboBox()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT id, nombre FROM tipos_relaciones_familiares";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string tipoRelacion = $"{reader["id"]}: {reader["nombre"]}";
                                comboBoxTipoParentesco.Items.Add(tipoRelacion);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void comboBoxParentescoPersonaje1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedFullName = comboBoxParentescoPersonaje1.SelectedItem.ToString();
            string[] nameParts = selectedFullName.Split(new string[] { ": " }, StringSplitOptions.None);
            if (nameParts.Length < 2)
            {
                MessageBox.Show("Formato de nombre no válido.");
                return;
            }

            if (!int.TryParse(nameParts[0], out int selectedId))
            {
                MessageBox.Show("ID de personaje no válido.");
                return;
            }

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT p2.nombre || ' ' || p2.apellido AS Parentesco, tr.nombre AS TipoRelacion " +
                                   "FROM parentescos p " +
                                   "JOIN personajes p2 ON p.personaje_id2 = p2.id " +
                                   "JOIN tipos_relaciones_familiares tr ON p.tipo_relacion_id = tr.id " +
                                   "WHERE p.personaje_id1 = @PersonajeId";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PersonajeId", selectedId);
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            comboBoxParentescos.Items.Clear();
                            while (reader.Read())
                            {
                                string parentesco = $"{reader["TipoRelacion"]} ({reader["Parentesco"]})";
                                comboBoxParentescos.Items.Add(parentesco);
                            }
                        }
                    }
                    if (comboBoxParentescos.Items.Count > 0)
                    {
                        comboBoxParentescos.Visible = true;
                        comboBoxParentescos.DroppedDown = true; // Despliega el ComboBox automáticamente
                                                                // MessageBox.Show("Parentescos cargados correctamente.");
                    }
                    else
                    {
                        comboBoxParentescos.Visible = false;
                        MessageBox.Show("No se encontraron parentescos para el personaje seleccionado.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void comboBoxTipoRelaccionEsposos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxTipoParentesco.SelectedItem.ToString().Contains("esposos"))
            {
                textBoxParentescoFecha_inicio.Visible = true;
                textBoxParentescoFecha_fin.Visible = true;
            }
            else
            {
                textBoxParentescoFecha_inicio.Visible = false;
                textBoxParentescoFecha_fin.Visible = false;
            }
        }

        private void buttonGuardarParentesco_Click(object sender, EventArgs e)
        {
            if (!ValidarFechas(out DateTime? fechaInicio, out DateTime? fechaFin))
            {
                return;
            }

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO parentescos (personaje_id1, personaje_id2, tipo_relacion_id, fecha_inicio, fecha_fin) VALUES (@personaje_id1, @personaje_id2, @tipo_relacion_id, @fecha_inicio, @fecha_fin)";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        int personajeId1 = int.Parse(comboBoxParentescoPersonaje1.SelectedItem.ToString().Split(':')[0]);
                        int personajeId2 = int.Parse(comboBoxParentescoPersonaje2.SelectedItem.ToString().Split(':')[0]);
                        int tipoRelacionId = int.Parse(comboBoxTipoParentesco.SelectedItem.ToString().Split(':')[0]);

                        command.Parameters.AddWithValue("@personaje_id1", personajeId1);
                        command.Parameters.AddWithValue("@personaje_id2", personajeId2);
                        command.Parameters.AddWithValue("@tipo_relacion_id", tipoRelacionId);

                        if (fechaInicio.HasValue)
                        {
                            command.Parameters.AddWithValue("@fecha_inicio", fechaInicio.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@fecha_inicio", DBNull.Value);
                        }

                        if (fechaFin.HasValue)
                        {
                            command.Parameters.AddWithValue("@fecha_fin", fechaFin.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@fecha_fin", DBNull.Value);
                        }

                        command.ExecuteNonQuery();
                    }

                    // Consulta para obtener la relación recíproca
                    string queryReciproca = "SELECT reciproca FROM tipos_relaciones_familiares WHERE id = @tipo_relacion_id";
                    using (SQLiteCommand commandReciproca = new SQLiteCommand(queryReciproca, connection))
                    {
                        int personajeId1 = int.Parse(comboBoxParentescoPersonaje1.SelectedItem.ToString().Split(':')[0]);
                        int personajeId2 = int.Parse(comboBoxParentescoPersonaje2.SelectedItem.ToString().Split(':')[0]);
                        int tipoRelacionId = int.Parse(comboBoxTipoParentesco.SelectedItem.ToString().Split(':')[0]);

                        commandReciproca.Parameters.AddWithValue("@tipo_relacion_id", tipoRelacionId);
                        object result = commandReciproca.ExecuteScalar();

                        if (result != null)
                        {
                            string resultString = result.ToString();
                            MessageBox.Show($"Valor de result: {resultString}");

                            // Realizar una consulta adicional para obtener el ID de la relación recíproca
                            string queryReciprocaId = "SELECT id FROM tipos_relaciones_familiares WHERE nombre = @nombre";
                            using (SQLiteCommand commandReciprocaId = new SQLiteCommand(queryReciprocaId, connection))
                            {
                                commandReciprocaId.Parameters.AddWithValue("@nombre", resultString);
                                object reciprocaIdResult = commandReciprocaId.ExecuteScalar();

                                if (reciprocaIdResult != null && int.TryParse(reciprocaIdResult.ToString(), out int tipoRelacionReciproca))
                                {
                                    // Insertar el parentesco recíproco
                                    string queryReciprocaInsert = "INSERT INTO parentescos (personaje_id1, personaje_id2, tipo_relacion_id, fecha_inicio, fecha_fin) VALUES (@personaje_id1, @personaje_id2, @tipo_relacion_id, @fecha_inicio, @fecha_fin)";
                                    using (SQLiteCommand commandReciprocaInsert = new SQLiteCommand(queryReciprocaInsert, connection))
                                    {
                                        commandReciprocaInsert.Parameters.AddWithValue("@personaje_id1", personajeId2);
                                        commandReciprocaInsert.Parameters.AddWithValue("@personaje_id2", personajeId1);
                                        commandReciprocaInsert.Parameters.AddWithValue("@tipo_relacion_id", tipoRelacionReciproca);

                                        if (fechaInicio.HasValue)
                                        {
                                            commandReciprocaInsert.Parameters.AddWithValue("@fecha_inicio", fechaInicio.Value);
                                        }
                                        else
                                        {
                                            commandReciprocaInsert.Parameters.AddWithValue("@fecha_inicio", DBNull.Value);
                                        }

                                        if (fechaFin.HasValue)
                                        {
                                            commandReciprocaInsert.Parameters.AddWithValue("@fecha_fin", fechaFin.Value);
                                        }
                                        else
                                        {
                                            commandReciprocaInsert.Parameters.AddWithValue("@fecha_fin", DBNull.Value);
                                        }

                                        commandReciprocaInsert.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("No se encontró un ID de relación recíproca válido.");
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("No se encontró una relación recíproca válida.");
                        }
                    }

                    MessageBox.Show("Relación y relación recíproca agregadas exitosamente.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        //------------------------------- relaciones sociales y políticas

        private void LoadRelativosComboBox() // puebla los comboBoxPersonaje1 y comboBoxPersonaje2 con los nombres y apellidos de los personajes
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT id, nombre, apellido FROM personajes";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string fullName = $"{reader["id"]}: {reader["nombre"]} {reader["apellido"]}";
                                comboBoxRelacionado1.Items.Add(fullName);
                                comboBoxRelacionado2.Items.Add(fullName);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void LoadTipoRelacionComboBox()
        {
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT id, nombre FROM tipos_relaciones_sociopoliticas";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string tipoRelacion = $"{reader["id"]}: {reader["nombre"]}";
                                comboBoxTipoRelacion.Items.Add(tipoRelacion);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void ButtonGuardarRelacionSP_Click(object sender, EventArgs e)
        {
            if (!ValidarFechas(out DateTime? fechaInicio, out DateTime? fechaFin))
            {
                return;
            }

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO relaciones_sociopoliticas (personaje_id1, personaje_id2, tipo_relacion_id, fecha_inicio, fecha_fin) VALUES (@personaje_id1, @personaje_id2, @tipo_relacion_id, @fecha_inicio, @fecha_fin)";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        int personajeId1 = int.Parse(comboBoxRelacionado1.SelectedItem.ToString().Split(':')[0]);
                        int personajeId2 = int.Parse(comboBoxRelacionado2.SelectedItem.ToString().Split(':')[0]);
                        int tipoRelacionId = int.Parse(comboBoxTipoRelacion.SelectedItem.ToString().Split(':')[0]);

                        command.Parameters.AddWithValue("@personaje_id1", personajeId1);
                        command.Parameters.AddWithValue("@personaje_id2", personajeId2);
                        command.Parameters.AddWithValue("@tipo_relacion_id", tipoRelacionId);

                        if (fechaInicio.HasValue)
                        {
                            command.Parameters.AddWithValue("@fecha_inicio", fechaInicio.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@fecha_inicio", DBNull.Value);
                        }

                        if (fechaFin.HasValue)
                        {
                            command.Parameters.AddWithValue("@fecha_fin", fechaFin.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@fecha_fin", DBNull.Value);
                        }

                        command.ExecuteNonQuery();
                    }

                    // Consulta para obtener la relación recíproca
                    string queryReciproca = "SELECT reciproca FROM tipos_relaciones_sociopoliticas WHERE id = @tipo_relacion_id";
                    using (SQLiteCommand commandReciproca = new SQLiteCommand(queryReciproca, connection))
                    {
                        int personajeId1 = int.Parse(comboBoxRelacionado1.SelectedItem.ToString().Split(':')[0]);
                        int personajeId2 = int.Parse(comboBoxRelacionado2.SelectedItem.ToString().Split(':')[0]);
                        int tipoRelacionId = int.Parse(comboBoxTipoRelacion.SelectedItem.ToString().Split(':')[0]);

                        commandReciproca.Parameters.AddWithValue("@tipo_relacion_id", tipoRelacionId);
                        object result = commandReciproca.ExecuteScalar();

                        if (

    }
}






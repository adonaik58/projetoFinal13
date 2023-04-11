import API from "../API/API.js";

const backSidebar = document.querySelector(".panel-insert-car");
const Form = document.querySelector(".panel-insert-car form");
const closeWindowCreateUser = document.querySelector(".panel-insert-car .head-panel div > button");
const insertNewUserInSpace = document.querySelector(".panel-insert-car form .more-field .field button");
const Paneltitle = document.querySelector(".panel-insert-car h1");
const cardSpace = document.querySelectorAll(".spaces-estacion .card");
const closeTicket = document.querySelector(".field .close_ticket");

closeWindowCreateUser.onclick = (e) => {
  if (e.target.tagName === "BUTTON" || e.target.tagName === "I") {
    backSidebar.classList.remove("active");
  }
};
space();

async function space() {
  const spaceCOntent = document.querySelector(".spaces-estacion");
  const selectMarca = document.querySelector(".panel-insert-car .more-field .field select#marca");
  const selectModelo = document.querySelector(".panel-insert-car .more-field .field select#modelo");

  const response = await API.spaceService.getSpaces();
  const marca = await API.spaceService.getMarca();
  let stringMarca = "";
  if (marca) {
    marca.map((el) => {
      stringMarca += `<option value="${el.id}">${el.nome}</option>`;
    });
    selectMarca.innerHTML = "<option>Escolher a Marca</option>" + stringMarca;
  }
  console.log(selectMarca);
  selectMarca.onchange = async (e) => {
    const response = await API.spaceService.getModelo(+e.target.value);
    let stringModelo = "";
    // console.log(response);
    if (response) {
      response.map((el) => {
        stringModelo += `<option value="${el.id}">${el.nome}</option>`;
      });
      selectModelo.innerHTML = "<option>Escolher a Marca</option>" + stringModelo;
    }
  };
  localStorage.setItem("consumers", JSON.stringify(response.filter((r) => r.estado === "i")));
  console.log(marca);
  let stringReceiving = "";
  let status = "";
  let cor = "";
  response.map((el, i) => {
    switch (el.estado) {
      case "i":
        status = "Ocupado";
        cor = "#ff0000";
        break;
      case "a":
        status = "Livre";
        cor = "#1ada74";
        break;
      case "m":
        status = "Indisponível";
        cor = "#ecb900";
        break;

      default:
        status = "???";
        break;
    }
    var tempo = el.tempo_ocupado;
    var temporizador =
      tempo.ano > 0
        ? `${tempo.ano} ano/s e ${tempo.mes} mês/meses`
        : tempo.mes > 0
        ? `${tempo.mes}m e ${tempo.dia}d`
        : tempo.dia > 0
        ? `${tempo.dia}d e ${tempo.hora}h`
        : tempo.hora > 0
        ? `${tempo.hora}h e ${tempo.minuto}min `
        : tempo.minuto > 0
        ? `${tempo.minuto}min e ${tempo.segundo}seg`
        : tempo.segundo > 0
        ? `${tempo.segundo}seg`
        : "";
    stringReceiving += `
      <div class="card" style="background: ${cor};" data-id='${el.id}'>
        <div style='display: flex; justify-content: space-between;'>
          <p class='name' data-id='${el.id}'><strong>${el.nome}</strong></p>
          ${el.preco ? `<p><strong>${el.preco}kz</strong></p>` : ""}
        </div>
        <h1>${status}</h1>
        <div class="card-code">
            <h3>${temporizador ?? "-"}</h3>
        </div>  
      </div>
    `;
  });
  spaceCOntent.innerHTML = stringReceiving;
  setInterval(async () => {
    const response = await API.spaceService.getSpaces();

    localStorage.setItem("consumers", JSON.stringify(response.filter((r) => r.estado === "i")));

    let stringReceiving = "";
    let status = "";
    let cor = "";
    response.map((el, i) => {
      switch (el.estado) {
        case "i":
          status = "Ocupado";
          cor = "#ff0000";
          break;
        case "a":
          status = "Livre";
          cor = "#1ada74";
          break;
        case "m":
          status = "Indisponível";
          cor = "#ecb900";
          break;

        default:
          status = "???";
          break;
      }
      var tempo = el.tempo_ocupado;
      var temporizador =
        tempo.ano > 0
          ? `${tempo.ano} ano/s e ${tempo.mes} mês/meses`
          : tempo.mes > 0
          ? `${tempo.mes}m e ${tempo.dia}d`
          : tempo.dia > 0
          ? `${tempo.dia}d e ${tempo.hora}h`
          : tempo.hora > 0
          ? `${tempo.hora}h e ${tempo.minuto}min `
          : tempo.minuto > 0
          ? `${tempo.minuto}min e ${tempo.segundo}seg`
          : tempo.segundo > 0
          ? `${tempo.segundo}seg`
          : "";

      stringReceiving += `
        <div class="card" style="background: ${cor};" data-id='${el.id}'>
          <div style='display: flex; justify-content: space-between;'>
            <p class='name' data-id='${el.id}'><strong>${el.nome}</strong></p>
            ${el.preco ? `<p><strong>${el.preco}kz</strong></p>` : ""}
          </div>
          <h1>${status}</h1>
          <div class="card-code">
              <h3>${temporizador ?? "-"}</h3>
          </div>
        </div>
      `;
    });
    spaceCOntent.innerHTML = stringReceiving;
    //-------------------------------------------
    spaceCOntent.querySelectorAll(".card").forEach((card) => {
      card.onclick = (e) => {
        const spaceID = card.dataset.id;

        const data = JSON.parse(localStorage.getItem("consumers"));

        const dataConsumer = data.find((kj) => +kj.id === +spaceID);
        closeTicket.setAttribute("data-id", `${dataConsumer.id}.${dataConsumer.bi}`);

        if (dataConsumer) {
          backSidebar.querySelector("form").style.display = "none";
          backSidebar.querySelector(".details").style.display = "block";
          Paneltitle.textContent = "Detalhes";
          showForm();
          const ul = backSidebar.querySelector(".details ul");
          const total = backSidebar.querySelector(".details h1");

          let string = `
            <li>
              <p><strong>Nome:</strong></p>
              <p>${dataConsumer.c_nome}</p>
            </li>
            <li>
              <p><strong>Idade:</strong></p>
              <p>${dataConsumer.idade.ano} anos</p>
            </li>
            <li>
              <p><strong>Bilhete de identidade:</strong></p>
              <p>${dataConsumer.bi}</p>
            </li>
            <li>
              <p><strong>Marca:</strong></p>
              <p>${dataConsumer.marca || "N/A"}</p>
            </li>
            <li>
              <p><strong>Modelo:</strong></p>
              <p>${dataConsumer.modelo || "N/A"}</p>
            </li>
            <li>
              <p><strong>Matrícula:</strong></p>
              <p>${dataConsumer.matricula}</p>
            </li>
            <li>
              <p><strong>Cor:</strong></p>
              <div style='width: 100px; heigth: 30px; background: ${dataConsumer.cor};'></div>
            </li>
          `;
          total.textContent = "Total: " + dataConsumer.preco + "kz";
          ul.innerHTML = string;
        } else {
          backSidebar.querySelector("form").style.display = "block";
          backSidebar.querySelector(".details").style.display = "none";
          showForm();
          Paneltitle.textContent = "Espaço - " + card.querySelector(".name").textContent;
        }

        const space = document.querySelector("input[name=spaceID]");
        space.value = card.dataset.id;
      };
    });
    //-------------------------------------------
  }, 10 * 1000);
  spaceCOntent.querySelectorAll(".card").forEach((card) => {
    card.onclick = (e) => {
      const spaceID = card.dataset.id;

      // console.log(spaceID);
      const data = JSON.parse(localStorage.getItem("consumers"));

      const dataConsumer = data.find((kj) => +kj.id === +spaceID);
      closeTicket.setAttribute("data-id", `${dataConsumer.id}.${dataConsumer.bi}`);

      if (dataConsumer) {
        backSidebar.querySelector("form").style.display = "none";
        backSidebar.querySelector(".details").style.display = "block";
        Paneltitle.textContent = "Detalhes";
        showForm();
        const ul = backSidebar.querySelector(".details ul");
        const total = backSidebar.querySelector(".details h1");

        let string = `
          <li>
            <p><strong>Nome:</strong></p>
            <p>${dataConsumer.c_nome}</p>
          </li>
          <li>
            <p><strong>Idade:</strong></p>
            <p>${dataConsumer.idade.ano} anos</p>
          </li>
          <li>
            <p><strong>Bilhete de identidade:</strong></p>
            <p>${dataConsumer.bi}</p>
          </li>
          <li>
            <p><strong>Marca:</strong></p>
            <p>${dataConsumer.marca || "N/A"}</p>
          </li>
          <li>
            <p><strong>Modelo:</strong></p>
            <p>${dataConsumer.modelo || "N/A"}</p>
          </li>
          <li>
            <p><strong>Matrícula:</strong></p>
            <p>${dataConsumer.matricula}</p>
          </li>
          <li>
            <p><strong>Cor:</strong></p>
            <div style='width: 100px; heigth: 30px; background: ${dataConsumer.cor};'></div>
          </li>
        `;
        total.textContent = "Total: " + dataConsumer.preco + "kz";
        ul.innerHTML = string;
      } else {
        backSidebar.querySelector("form").style.display = "block";
        backSidebar.querySelector(".details").style.display = "none";
        showForm();
        Paneltitle.textContent = "Espaço - " + card.querySelector(".name").textContent;
      }

      const space = document.querySelector("input[name=spaceID]");
      space.value = card.dataset.id;
    };
  });
}

function showForm() {
  backSidebar.classList.add("active");
}
insertNewUserInSpace.onclick = async () => {
  const space = document.querySelector("input[name=spaceID]");
  const formData = new FormData(Form);
  formData.append("spaceID", space.value);
  const response = await API.spaceService.atributeUser(formData);
  console.log(response.data);
  FNtoast(response);

  // space();
};

/* var allSpace = [];
const alphabet = "ABCDEFGHIJKLMNOQRSTUVWXYZ".split("");
for (var i = 0; i < alphabet.length; i++){
    var t = 1;
    for(; t <= 10; t++){
        allSpace.push(alphabet[i]+t)
    }
} */

function FNtoast(response) {
  const toast = document.querySelector(".drt .toast");
  const icon = toast.querySelector(".icon i");

  toast.classList.add(response.status ? "success" : "error");
  icon.classList.add(response.status ? "fa-check" : "fa-times");

  toast.querySelector(".text p").textContent = response.message;
  setTimeout(() => {
    icon.classList.remove(response.status ? "fa-check" : "fa-times");
    toast.classList.remove(response.status ? "success" : "error");
    console.log(toast.classList);
  }, 6000);
}

closeTicket.onclick = async () => {
  const id = closeTicket.dataset.id;
  const Ok = await API.spaceService.closeTicket(id);

  FNtoast(Ok);
};

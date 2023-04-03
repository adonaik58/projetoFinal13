import API from "../API/API.js";

const backSidebar = document.querySelector(".panel-insert-car");
const Form = document.querySelector(".panel-insert-car form");
const closeWindowCreateUser = document.querySelector(".panel-insert-car .head-panel div > button");
const insertNewUserInSpace = document.querySelector(".panel-insert-car form .more-field .field button");
const Paneltitle = document.querySelector(".panel-insert-car h1");
const cardSpace = document.querySelectorAll(".spaces-estacion .card");

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
  selectModelo.setAttribute("disabled", true);

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
      selectModelo.removeAttribute("disabled");
    }
  };
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
    stringReceiving += `
      <div class="card" style="background: ${cor};">
        ${el.preco ? `<p><strong>${el.preco}kz</strong></p>` : ""}
        <p class='name' data-id='${el.id}'><strong>${el.nome}</strong></p>
        <h1>${status}</h1>
        <div class="card-code">
            <h3>${el.codigo}</h3>
        </div>
      </div>
    `;
  });
  spaceCOntent.innerHTML = stringReceiving;
  spaceCOntent.querySelectorAll(".card").forEach((card) => {
    card.onclick = (e) => {
      showForm();
      // const formData = new FormData(Form);
      //   const response = await API.spaceService.newSpace();
      Paneltitle.textContent = "Espaço - " + card.querySelector(".name").textContent;
    };
  });
}

function showForm() {
  backSidebar.classList.add("active");
}
// insertNewUserInSpace.onclick = async () => {
//   const formData = new FormData(Form);
//   const response = await API.spaceService.newSpace();

//   console.log(response);
// };

/* var allSpace = [];
const alphabet = "ABCDEFGHIJKLMNOQRSTUVWXYZ".split("");
for (var i = 0; i < alphabet.length; i++){
    var t = 1;
    for(; t <= 10; t++){
        allSpace.push(alphabet[i]+t)
    }
} */

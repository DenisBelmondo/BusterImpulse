# TODO

- find out how tf to refactor this
    - model view controller style?
    - model - game state
    - view - renderer
    - controller - uhh, controllers
    - controllers should usually just be static classes whose first argument is
    - the data they want to operate on. if they need dependencies, they should
      be a regular class then.

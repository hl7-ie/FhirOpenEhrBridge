module.exports = {
  default: {
    require: ['test/bdd/steps/**/*.ts'],
    requireModule: ['ts-node/register'],
    paths: ['test/bdd/features/**/*.feature'],
  },
};
